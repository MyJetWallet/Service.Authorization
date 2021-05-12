using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using OpenTelemetry.Trace;
using Service.Authorization.Domain.Models;
using Service.Authorization.Domain.Models.NoSql;
using Service.Authorization.Grpc;
using Service.Authorization.Grpc.Models;
using Service.Authorization.Settings;
using Service.ClientWallets.Domain.Models;
using Service.ClientWallets.Grpc;
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;

namespace Service.Authorization.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ILogger<AuthorizationService> _logger;
        private readonly SettingsModel _settings;
        private readonly IMyNoSqlServerDataWriter<SpotSessionNoSql> _writer;
        private readonly IClientRegistrationService _clientRegistrationService;
        private readonly IClientWalletService _clientWalletService;
        private readonly ISessionAuditService _sessionAuditService;

        public AuthorizationService(
            ILogger<AuthorizationService> logger, SettingsModel settings, 
            IMyNoSqlServerDataWriter<SpotSessionNoSql> writer, 
            IClientRegistrationService clientRegistrationService,
            IClientWalletService clientWalletService,
            ISessionAuditService sessionAuditService)
        {
            _logger = logger;
            _settings = settings;
            _writer = writer;
            _clientRegistrationService = clientRegistrationService;
            _clientWalletService = clientWalletService;
            _sessionAuditService = sessionAuditService;
        }


        public async Task<AuthorizationResponse> AuthorizationAsync(AuthorizationRequest request)
        {
            using var activity = MyTelemetry.StartActivity("Authorization base on session token");

            if (string.IsNullOrEmpty(request.Token) ||
                string.IsNullOrEmpty(request.BrandId) ||
                string.IsNullOrEmpty(request.BrokerId))
            {
                return new AuthorizationResponse() {Result = false};
            }

            var (result, baseToken) = TokensManager.ParseBase64Token<JetWalletToken>(request.Token, AuthConst.GetSessionEncodingKey(), DateTime.UtcNow);

            if (result != TokenParseResult.Ok)
            {
                activity.SetStatus(Status.Error);
                return new AuthorizationResponse() { Result = false };
            }

            if (!string.IsNullOrEmpty(baseToken.SessionRootId))
            {
                _logger.LogWarning("Cannot Authorization session base on token with existing RootSession: {rootIdText}", baseToken.SessionRootId);
                activity.SetStatus(Status.Error);
                return new AuthorizationResponse() { Result = false };
            }

            var token = new JetWalletToken()
            {
                Id = baseToken.Id,
                Expires = DateTime.UtcNow.AddMinutes(_settings.SessionLifeTimeMinutes),
                SessionRootId = Guid.NewGuid().ToString("N"),
                SessionId = Guid.NewGuid().ToString("N"),
                BrandId = request.BrandId,
                BrokerId = request.BrokerId
            };

            token.Id.AddToActivityAsTag("clientId");
            token.BrokerId.AddToActivityAsTag("brokerId");
            token.BrandId.AddToActivityAsTag("brandId");

            token.SessionRootId.AddToActivityAsTag("sessionRootId");


            var clientIdentity = new JetClientIdentity(request.BrokerId, request.BrandId, baseToken.Id);
            var response = await _clientRegistrationService.GetOrRegisterClientAsync(clientIdentity);
            if (response.Result != ClientRegistrationResponse.RegistrationResult.Ok)
            {
                _logger.LogError("Cannot register client. Client already register with another brand. BrokerId/BrandId/ClientId: {brokerId}/{brandId}/{clientId}",
                    clientIdentity.BrokerId, clientIdentity.BrandId, clientIdentity.ClientId);

                activity.SetStatus(Status.Error);
                return new AuthorizationResponse() { Result = false };
            }

            ClientWallet wallet = null;
            var wallets = await _clientWalletService.GetWalletsByClient(clientIdentity);
            if (string.IsNullOrEmpty(request.WalletId))
            {
                wallet = wallets?.Wallets?.FirstOrDefault(w => w.IsDefault) ?? wallets?.Wallets?.FirstOrDefault();
            }
            else
            {
                wallet = wallets?.Wallets?.FirstOrDefault(w => w.WalletId == request.WalletId);
            }

            if (wallet == null)
            {
                request.WalletId.AddToActivityAsTag("walletId");
                _logger.LogWarning("Cannot Authorization session, wallet do not found. WalletId {walletId}. ClientId: {clientId}", request.WalletId, token.Id);
                activity.SetStatus(Status.Error);
                return new AuthorizationResponse() { Result = false };
            }

            token.WalletId = wallet.WalletId;
            token.WalletId.AddToActivityAsTag("walletId");

            var session = token.IssueTokenAsBase64String(AuthConst.GetSessionEncodingKey());

            var dueData = DateTime.UtcNow.AddHours(_settings.RootSessionLifeTimeHours);
            var publicKey = MyRsa.ReadPublicKeyFromPem(request.PublicKeyPem);

            var entity = SpotSessionNoSql.Create(request.BrokerId, request.BrandId, baseToken.Id, dueData, publicKey, token.SessionRootId);
            await _writer.InsertOrReplaceAsync(entity);

            await _sessionAuditService.NewSessionAudit(baseToken, token, request.UserAgent, request.Ip);

            _logger.LogInformation("Session Authorization is success. RootSessionId: {rootIdText}. ClientId:{clientId}", token.SessionRootId, token.ClientId());

            return new AuthorizationResponse()
            {
                Result = true,
                Token = session
            };
        }

        public async Task KillRootSessionAsync(KillRootSessionRequest request)
        {
            using var activity = MyTelemetry.StartActivity("Kill Root Session");

            if (string.IsNullOrEmpty(request.SessionRootId) || string.IsNullOrEmpty(request.ClientId))
            {
                _logger.LogWarning("Cannot kill session, RootSessionId is empty or ClientId is empty");
                activity.SetStatus(Status.Error);
                return;
            }

            await _writer.DeleteAsync(SpotSessionNoSql.GeneratePartitionKey(request.ClientId), SpotSessionNoSql.GenerateRowKey(request.SessionRootId));

            request.ClientId.AddToActivityAsTag("clientId");

            request.SessionRootId.AddToActivityAsTag("sessionRootId");

            await _sessionAuditService.KillSessionAudit(request.SessionRootId, request.SessionId, request.ClientId, request.Reason, request.UserAgent, request.Ip);
            _logger.LogInformation("Session is killed. ClientId: {clientId}, RootSessionId: {rootIdText}", request.ClientId, request.SessionRootId);
        }

        public async Task<AuthorizationResponse> RefreshSessionAsync(RefreshSessionRequest request)
        {
            using var activity = MyTelemetry.StartActivity("Refresh Session");

            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.SignatureBase64))
            {
                activity.AddTag("message", "bad request");
                activity.SetStatus(Status.Error);

                return new AuthorizationResponse()
                {
                    Result = false
                };
            }

            if ( DateTime.UtcNow < request.RequestTimestamp || request.RequestTimestamp < DateTime.UtcNow.AddSeconds(-_settings.RequestTimeLifeSec))
            {
                activity.AddTag("message", "request expired");
                activity.SetStatus(Status.Error);

                return new AuthorizationResponse()
                {
                    Result = false
                };
            }

            var (result, token) = TokensManager.ParseBase64Token<JetWalletToken>(request.Token, AuthConst.GetSessionEncodingKey(), DateTime.UtcNow);

            if (result != TokenParseResult.Ok && result != TokenParseResult.Expired)
            {
                activity.AddTag("message", "wrong token");
                activity.SetStatus(Status.Error);

                return new AuthorizationResponse()
                {
                    Result = false
                };
            }

            token.Id.AddToActivityAsTag("clientId");
            token.BrokerId.AddToActivityAsTag("brokerId");
            token.BrandId.AddToActivityAsTag("brandId");
            token.WalletId.AddToActivityAsTag("walletId");
            token.SessionRootId.AddToActivityAsTag("sessionRootId");

            var entity = await _writer.GetAsync(SpotSessionNoSql.GeneratePartitionKey(token.ClientId()), SpotSessionNoSql.GenerateRowKey(token.SessionRootId));
            if (entity == null)
            {
                activity.AddTag("message", "root session do not exist");
                activity.SetStatus(Status.Error);

                return new AuthorizationResponse()
                {
                    Result = false
                };
            }

            if (DateTime.UtcNow >= entity.DiedDateTime)
            {
                activity.AddTag("message", "root session is died");
                activity.SetStatus(Status.Error);

                return new AuthorizationResponse()
                {
                    Result = false
                };
            }

            if (DateTime.UtcNow <= entity.CreateDateTime.AddSeconds(_settings.TimeoutToRefreshNewSessionInSec))
            {
                activity.AddTag("message", "the session is very young, for renewal");
                activity.SetStatus(Status.Error);

                return new AuthorizationResponse()
                {
                    Result = false
                };
            }

            var signContent = $"{request.Token}_{request.RequestTimestamp:yyyy-MM-ddTHH:mm:ss}_{request.NewWalletId}";
            var verifySignature = MyRsa.ValidateSignature(signContent, request.SignatureBase64, entity.PublicKeyBase64);

            if (!verifySignature)
            {
                activity.AddTag("message", "wrong signature");
                activity.SetStatus(Status.Error);

                return new AuthorizationResponse()
                {
                    Result = false
                };
            }

            var walletId = token.WalletId;

            if (!string.IsNullOrEmpty(request.NewWalletId))
            {
                var clientIdentity = new JetClientIdentity(token.BrokerId, token.BrandId, token.Id);
                var wallets = await _clientWalletService.GetWalletsByClient(clientIdentity);

                var wallet = wallets?.Wallets?.FirstOrDefault(w => w.WalletId == request.NewWalletId);

                if (wallet == null)
                {
                    request.NewWalletId.AddToActivityAsTag("walletId");
                    _logger.LogWarning("Cannot Refresh session, NewWallet do not found. WalletId {walletId}. ClientId: {clientId}", request.NewWalletId, token.Id);
                    activity.SetStatus(Status.Error);
                    return new AuthorizationResponse() { Result = false };
                }

                walletId = wallet.WalletId;
                _logger.LogInformation("Client update session to new walletId. SessionRootId: {sessionRootId}; ClientId: {clientId}; WalletId: {walletId}",
                    token.SessionRootId, token.Id, walletId);
            }

            walletId.AddToActivityAsTag("walletId");

            var newToken = new JetWalletToken()
            {
                Id = token.Id,
                Expires = DateTime.UtcNow.AddMinutes(_settings.SessionLifeTimeMinutes),
                SessionRootId = token.SessionRootId,
                SessionId = Guid.NewGuid().ToString("N"),
                BrandId = token.BrandId,
                BrokerId = token.BrokerId,
                WalletId = walletId
            };

            await _sessionAuditService.RefreshSessionAudit(token, newToken, request.UserAgent, request.Ip);

            _logger.LogInformation("Refresh session is success. SessionRootId: {sessionRootId}; SessionId: {sessionId}; PrevSessionId: {prevSessionId}; ClientId: {clientId}; WalletId: {walletId}",
                newToken.SessionRootId, newToken.SessionId, token.SessionId, newToken.ClientId(), newToken.WalletId);
            
            return new AuthorizationResponse()
            {
                Token = newToken.IssueTokenAsBase64String(AuthConst.GetSessionEncodingKey()),
                Result = true
            };
        }

        public async Task<ListResponse<SessionAudit>> GetActiveSessionsAsync(GetActiveSessionsRequest request)
        {
            var sessions = await _sessionAuditService.GetActiveSessions(request.ClientId);

            return new ListResponse<SessionAudit>()
            {
                List = sessions
            };

        }
    }
}
