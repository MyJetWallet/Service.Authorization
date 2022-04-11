using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Authorization.Domain.Models.ServiceBus;
using Service.Authorization.Grpc;
using Service.Authorization.Grpc.Contracts;
using Service.Authorization.NoSql;
using Service.Authorization.Postgres.Models;

namespace Service.Authorization.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly AuthenticationCredentialsCacheReader _authenticationCredentialsCacheReader;
        private readonly AuthenticationCredentialsCacheWriter _authenticationCredentialsCacheWriter;
        private readonly AuthenticationCredentialsRepository _authenticationCredentialsRepository;
        private readonly AuthLogQueue _authLogQueue;
        private readonly IServiceBusPublisher<ClientAuthenticationMessage> _publisher;
        private readonly IServiceBusPublisher<PasswordChangedMessage> _passwordChangePublisher;
        public AuthService(
            ILogger<AuthService> logger, 
            AuthenticationCredentialsCacheReader authenticationCredentialsCacheReader, 
            AuthenticationCredentialsCacheWriter authenticationCredentialsCacheWriter, 
            AuthenticationCredentialsRepository authenticationCredentialsRepository, 
            AuthLogQueue authLogQueue, 
            IServiceBusPublisher<ClientAuthenticationMessage> publisher)
        {
            _logger = logger;
            _authenticationCredentialsCacheReader = authenticationCredentialsCacheReader;
            _authenticationCredentialsCacheWriter = authenticationCredentialsCacheWriter;
            _authenticationCredentialsRepository = authenticationCredentialsRepository;
            _authLogQueue = authLogQueue;
            _publisher = publisher;
        }

        public async ValueTask<AuthenticateGrpcResponse> AuthenticateAsync(AuthenticateGrpcRequest request)
        {
            _logger.LogInformation("AuthenticateAsync {@Request}", request);
            var cacheResult = _authenticationCredentialsCacheReader.GetByEmail(request.Email, request.Brand);
            string traderId = null;

            if (cacheResult != null && cacheResult.Authenticate(request.Password))
            {
                traderId = cacheResult.Id;
            }
            else
            {
                var responseFromDb = await _authenticationCredentialsRepository
                    .GetByEmailAsync(request.Email, request.Brand);

                if (responseFromDb != null && responseFromDb.Authenticate(request.Password))
                {
                    await _authenticationCredentialsCacheWriter.PurgeCache(Program.Settings.MaxItemsInCache);
                    await _authenticationCredentialsCacheWriter.AddByDatabaseEntity(responseFromDb);
                    _logger.LogInformation("AuthenticateAsync.AddByDatabaseEntity {@Entity}", responseFromDb);

                    traderId = responseFromDb.Id;
                }
            }

            if (traderId == null) return new AuthenticateGrpcResponse { TraderId = traderId };
            
            _authLogQueue.HandleEvent(new AuthLogModelDbModel
            {
                TraderId = traderId,
                Ip = request.Ip,
                UserAgent = request.UserAgent,
                DateTime = DateTime.UtcNow,
                Location = request.Location
            });

            await SendAuthTriggerEvent(request, traderId);

            return new AuthenticateGrpcResponse { TraderId = traderId };
        }

        public async ValueTask<ExistsGrpcResponse> ExistsAsync(ExistsGrpcRequest request)
        {
            _logger.LogInformation("ExistsAsync {@Request}", request);
            var cacheResult = _authenticationCredentialsCacheReader.GetById(request.TraderId, request.Brand);

            if (cacheResult != null)
                return new ExistsGrpcResponse { Yes = true };

            var result = await _authenticationCredentialsRepository.GetByIdAsync(request.TraderId);

            var isExists = result != null;


            if (isExists)
            {
                await _authenticationCredentialsCacheWriter.PurgeCache(Program.Settings.MaxItemsInCache);
                await _authenticationCredentialsCacheWriter.AddEncodedByDatabaseEntity(result);
                _logger.LogInformation("ExistsAsync.AddEncodedByDatabaseEntity {@Entity}", result);
            }

            return new ExistsGrpcResponse
            {
                Yes = isExists
            };
        }

        public async ValueTask<GetIdGrpcResponse> GetIdByEmailAsync(GetIdGrpcRequest request)
        {
            _logger.LogInformation("GetIdByEmailAsync {@Request}", request);
            var cacheResult = _authenticationCredentialsCacheReader.GetByEmail(request.Email, request.Brand);
            if (cacheResult != null)
                return new GetIdGrpcResponse { TraderId = cacheResult.Id };

            var responseFromDb = await _authenticationCredentialsRepository.GetByEmailAsync(request.Email, request.Brand);
            if (responseFromDb == null)
                return new GetIdGrpcResponse { TraderId = null };

            await _authenticationCredentialsCacheWriter.PurgeCache(Program.Settings.MaxItemsInCache);
            await _authenticationCredentialsCacheWriter.AddByDatabaseEntity(responseFromDb);
            _logger.LogInformation("GetIdByEmailAsync.AddByDatabaseEntity {@Entity}", responseFromDb);

            return new GetIdGrpcResponse
            {
                TraderId = responseFromDb.Id
            };
        }

        public async ValueTask<GetIdsByEmailGrpcResponse> GetIdsByEmailAsync(GetIdsByEmailGrpcRequest request)
        {
            _logger.LogInformation("GetIdsByEmailAsync {@Request}", request);
            var traderBrands = new List<TraderBrandGrpcModel>();

            var traders = await _authenticationCredentialsRepository.GetTradersByEmailAsync(request.Email);

            foreach (var trader in traders)
            {
                traderBrands.Add(new TraderBrandGrpcModel
                {
                    TraderId = trader.Id,
                    Brand = trader.Brand
                });
            }

            return new GetIdsByEmailGrpcResponse { TraderBrands = traderBrands };
        }

        public async ValueTask<GetEmailByIdGrpcResponse> GetEmailByIdAsync(GetEmailByIdGrpcRequest request)
        {
            _logger.LogInformation("GetEmailByIdAsync {@Request}", request);
            var cacheResult = _authenticationCredentialsCacheReader.GetById(request.TraderId, request.Brand);

            if (cacheResult != null)
                return new GetEmailByIdGrpcResponse { Email = cacheResult.Email, Brand = cacheResult.Brand };

            var result = await _authenticationCredentialsRepository.GetByIdAsync(request.TraderId);
            if (result == null)
                return new GetEmailByIdGrpcResponse { Email = null, Brand = null };

            var decodedEmail = result.Email;
            await _authenticationCredentialsCacheWriter.PurgeCache(Program.Settings.MaxItemsInCache);
            await _authenticationCredentialsCacheWriter.AddEncodedByDatabaseEntity(result);
            _logger.LogInformation("GetEmailByIdAsync.AddEncodedByDatabaseEntity {@Entity}", result);

            return new GetEmailByIdGrpcResponse
            {
                Email = decodedEmail,
                Brand = result.Brand
            };
        }

        public async ValueTask<ChangePasswordGrpcResponse> ChangePasswordAsync(ChangePasswordGrpcContract request)
        {
            _logger.LogInformation("ChangePasswordAsync {@Request}", request);
            var newEntity = await _authenticationCredentialsRepository.ChangePasswordAsync(request.Email, request.Hash, request.Salt, request.Brand);
            if (newEntity == null)
                return new ChangePasswordGrpcResponse();

            await _authenticationCredentialsCacheWriter.PurgeCache(Program.Settings.MaxItemsInCache);
            await _authenticationCredentialsCacheWriter.AddByDatabaseEntity(newEntity);
            _logger.LogInformation("ChangePasswordAsync.AddByDatabaseEntity {@Entity}", newEntity);
            
            var message = new PasswordChangedMessage
            {
                TraderId = newEntity.Id,
                Brand = newEntity.Brand,
                DatePublish = DateTime.UtcNow
            };

            await _passwordChangePublisher.PublishAsync(message);

            return new ChangePasswordGrpcResponse();
        }

        public async ValueTask<ComparePasswordResponse> ComparePasswordAsync(ComparePasswordRequest request)
        {
            _logger.LogInformation("ComparePasswordAsync {@Request}", request);  
            var cacheResult = _authenticationCredentialsCacheReader.GetById(request.TraderId, request.Brand);
            if (cacheResult != null && cacheResult.Authenticate(request.Password))
                return new ComparePasswordResponse { Ok = true };

            var responseFromDb = await _authenticationCredentialsRepository.GetByIdAsync(request.TraderId);

            if (responseFromDb == null || !responseFromDb.Authenticate(request.Password))
                return new ComparePasswordResponse { Ok = false };

            await _authenticationCredentialsCacheWriter.PurgeCache(Program.Settings.MaxItemsInCache);
            await _authenticationCredentialsCacheWriter.AddEncodedByDatabaseEntity(responseFromDb);
            _logger.LogInformation("ComparePasswordAsync.AddByDatabaseEntity {@Entity}", responseFromDb);

            return new ComparePasswordResponse { Ok = true };
        }

        public async ValueTask ClearCacheAsync(ClearCacheRequest request)
        {
            _logger.LogInformation("ClearCacheAsync {@Request}", request);
            var cacheResult = _authenticationCredentialsCacheReader.GetByEmail(request.Email, request.Brand);

            if (cacheResult == null)
            {
                _logger.LogInformation("ClearCacheAsync: Not found in cache");
                return;
            }

            await _authenticationCredentialsCacheWriter.DeleteAsync(request.Email, request.Brand);

            _logger.LogInformation("ClearCacheAsync: Deleted from cache {@Request}", request);
        }

        public async ValueTask RegisterCredentialsAsync(AuthCredentialsGrpcModel entity)
        {
            _logger.LogInformation("RegisterCredentialsAsync {@Request}", entity);
            await _authenticationCredentialsRepository.AddCredentialsAsync(AuthenticationCredentialsEntity.Create(entity.Id, entity.EncodedEmail, entity.Hash, entity.Salt, entity.Brand));
            _logger.LogInformation("RegisterCredentialsAsync: Credential added {@Request}", entity);
        }

        public async ValueTask RemoveCredentialsAsync(RemoveCredentialsGrpcRequest request)
        {
            if (string.IsNullOrEmpty(request.ClientId))
                return;
            
            var (email, brand) = await _authenticationCredentialsRepository.RemoveCredentialsAsync(request.ClientId);

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(brand))
            {
                await _authenticationCredentialsCacheWriter.DeleteAsync(email, brand);
            }

            _logger.LogInformation("Credentials for client {clientId} is removed", request.ClientId);
        }

        private async ValueTask SendAuthTriggerEvent(AuthenticateGrpcRequest request, string traderId)
        {
            _logger.LogInformation("SendAuthTriggerEvent {@Request}", request);
            var message = new ClientAuthenticationMessage()
            {
                TraderId = traderId,
                Brand = request.Brand,
                Ip = request.Ip,
                LanguageId = request.LanguageId,
                UserAgent = request.UserAgent
            };

            await _publisher.PublishAsync(message);
        }
    }
}