using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.Authorization.Domain.Models;
using Service.Authorization.Domain.Models.NoSql;
using Service.Wallet.Api.Authentication;
using SimpleTrading.TokensManager;

namespace Service.Authorization.Client.Http
{
    public class WalletAuthHandler : AuthenticationHandler<WalletAuthenticationOptions>
    {
        private readonly IMyNoSqlServerDataReader<SpotSessionNoSql> _reader;
        public const string DefaultBroker = "jetwallet";
        public const string DefaultBrand = "default-brand";
        
        public const string ClientIdClaim = "Client-Id";
        public const string BrokerIdClaim = "Broker-Id";
        public const string BrandIdClaim = "Brand-Id";
        public const string WalletIdClaim = "Wallet-Id";

        public const string SessionIdClaim = "Session-Id";
        public const string SessionRootIdClaim = "Session-Root-Id";

        public const string SignatureClaim = "Request-Signature";
        
        public const string AuthorizationHeader = "authorization";
        private const string SignatureHeader = "signature";

        public WalletAuthHandler(IOptionsMonitor<WalletAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            IMyNoSqlServerDataReader<SpotSessionNoSql> reader) : base(options, logger, encoder, clock)
        {
            _reader = reader;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (!Context.Request.Headers.ContainsKey(AuthorizationHeader))
                    throw new UnauthorizedAccessException("UnAuthorized request");

                var itm = Context.Request.Headers[AuthorizationHeader].ToString().Trim();
                var items = itm.Split();
                var authToken = items[^1];

                Console.WriteLine($"Token: {authToken}");

                var (result, token) = TokensManager.ParseBase64Token<JetWalletToken>(authToken, AuthConst.GetSessionEncodingKey(), DateTime.UtcNow);

                token?.ClientId().AddToActivityAsTag("clientId");

                if (result != TokenParseResult.Ok)
                {
                    throw new UnauthorizedAccessException($"Wrong token: {result.ToString()}");
                }

                if (token == null)
                {
                    throw new UnauthorizedAccessException($"Wrong token: cannot parse token");
                }

                if (!token.IsValid())
                {
                    throw new UnauthorizedAccessException($"Wrong token: not valid");
                }

                var rootSession = _reader.Get(SpotSessionNoSql.GeneratePartitionKey(token.ClientId()), SpotSessionNoSql.GenerateRowKey(token.SessionRootId));
                if (rootSession == null)
                {
                    throw new UnauthorizedAccessException($"Wrong token: root session is not found");
                }

                var clientId = new JetClientIdentity(token.BrokerId, token.BrandId, token.Id);

                token.BrokerId.AddToActivityAsTag("brokerId");
                token.BrandId.AddToActivityAsTag("brandId");
                token.WalletId.AddToActivityAsTag("walletId");
                token.SessionId.AddToActivityAsTag("sessionId");
                token.SessionRootId.AddToActivityAsTag("sessionRootId");
                Activity.Current?.AddBaggage("client-id", token.BrokerId);
                Activity.Current?.AddBaggage("broker-id", token.ClientId());
                Activity.Current?.AddBaggage("brand-id", token.BrandId);
                Activity.Current?.AddBaggage("wallet-id", token.WalletId);


                var signature = string.Empty;

                if (Context.Request.Headers.ContainsKey(SignatureHeader))
                {
                    signature = Context.Request.Headers[SignatureHeader].ToString().Trim();
                }
                
                var identity = new GenericIdentity(clientId.ClientId);
                identity.AddClaim(new Claim(ClientIdClaim, token.ClientId()));
                identity.AddClaim(new Claim(BrokerIdClaim, token.BrokerId));
                identity.AddClaim(new Claim(BrandIdClaim, token.BrandId));
                identity.AddClaim(new Claim(WalletIdClaim, token.WalletId));
                identity.AddClaim(new Claim(SessionIdClaim, token.SessionId));
                identity.AddClaim(new Claim(SessionRootIdClaim, token.SessionRootId));
                if (!string.IsNullOrEmpty(signature))
                {
                    identity.AddClaim(new Claim(SignatureClaim, signature));
                }
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex);
                ex.FailActivity();
                return Task.FromResult(AuthenticateResult.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ex.FailActivity();
                return Task.FromResult(AuthenticateResult.Fail("unauthorized"));
            }
        }
    }
}