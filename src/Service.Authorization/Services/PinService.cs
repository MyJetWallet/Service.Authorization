using System;
using System.Buffers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using ProtoBuf.WellKnownTypes;
using Service.Authorization.Domain;
using Service.Authorization.Domain.Models;
using Service.Authorization.Grpc;
using Service.Authorization.Grpc.Contracts.PinDto;
using Service.Authorization.NoSql;
using Service.Authorization.Postgres;

namespace Service.Authorization.Services
{
    public class PinService: IPinService
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;
        private readonly ILogger<PinService> _logger;
        private readonly IMyNoSqlServerDataWriter<PinRecordNoSqlEntity> _writer;

        public PinService(
            DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder, 
            ILogger<PinService> logger,
            IMyNoSqlServerDataWriter<PinRecordNoSqlEntity> writer)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
            _logger = logger;
            _writer = writer;
        }

        public async Task SetupPinAsync(SetupPinGrpcRequest request)
        {
            _logger.LogInformation("Setup pin for client: {clientId}, data: {dataJson}", 
                request.ClientId,
                new {request.Ip, request.IpCountry, request.RootSessionId, request.SessionId, request.UserAgent}.ToJson());
            
            if (string.IsNullOrEmpty(request.Pin))
            {
                _logger.LogError("Receive call SetupPinAsync with empty Pin");
                throw new Exception("Receive call SetupPinAsync with empty Pin");
            }
            
            if (string.IsNullOrEmpty(request.ClientId))
            {
                _logger.LogError("Receive call SetupPinAsync with empty ClientId");
                throw new Exception("Receive call SetupPinAsync with empty ClientId");
            }

            var pin = new PinRecord()
            {
                ClientId = request.ClientId,
                Salt = Guid.NewGuid().ToString("N"),
                HasPinIssue = false,
                IsInited = true
            };
            pin.Hash = AuthHelper.GeneratePasswordHash(request.Pin, pin.Salt);
                
            
            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
                var session = await ctx.PinRecordSessionIssues
                    .FirstOrDefaultAsync(e => e.ClientId == pin.ClientId && e.RootSessionId == request.RootSessionId);
                
                if (session != null && session.CurrentFailAttempts > 0)
                {
                    session.CurrentFailAttempts = 0;
                    session.TotalFailAttempts = 0;
                    await ctx.SaveChangesAsync();
                }
                    
                await ctx.PinRecords.Upsert(pin).On(e => e.ClientId).RunAsync();
            }
            await _writer.InsertOrReplaceAsync(PinRecordNoSqlEntity.Create(pin));
        }

        public async Task RemovePinAsync(RemovePinGrpcRequest request)
        {
            _logger.LogInformation("Remove pin for client: {clientId}, data: {dataJson}", 
                request.ClientId, 
                new {request.Ip, request.IpCountry, request.RootSessionId, request.SessionId, request.UserAgent}.ToJson());
            
            var pin = new PinRecord()
            {
                ClientId = request.ClientId,
                Salt = string.Empty,
                HasPinIssue = false,
                IsInited = false
            };
            
            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
                
                await ctx.PinRecords.Upsert(pin).On(e => e.ClientId).RunAsync();

                var session = await ctx.PinRecordSessionIssues
                        .FirstOrDefaultAsync(e =>
                            e.ClientId == request.ClientId && e.RootSessionId == request.RootSessionId);

                if (session != null)
                {
                    session.CurrentFailAttempts = 0;
                    session.TotalFailAttempts = 0;
                }
                await ctx.SaveChangesAsync();
            }
            
            await _writer.InsertOrReplaceAsync(PinRecordNoSqlEntity.Create(pin));
        }

        public async Task<CheckPinGrpcResponse> CheckPinAsync(CheckPinGrpcRequest request)
        {
            _logger.LogInformation("Check pin for client: {clientId}, {dataJson}", 
                request.ClientId,
                new {request.Ip, request.IpCountry, request.RootSessionId, request.SessionId, request.UserAgent}.ToJson());
        
            PinRecord record = null;

            await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
            try
            {
                record = await ctx.PinRecords.FirstOrDefaultAsync(e => e.ClientId == request.ClientId);
                if (record == null)
                {
                    record = new PinRecord()
                    {
                        ClientId = request.ClientId,
                        Hash = string.Empty,
                        Salt = Guid.NewGuid().ToString("N"),
                        IsInited = false,
                        HasPinIssue = false
                    };
                    ctx.PinRecords.Add(record);
                }

                if (!record.IsInited)
                {
                    return new CheckPinGrpcResponse() {IsValid = true};
                }

                var session = await ctx.PinRecordSessionIssues
                    .FirstOrDefaultAsync(
                        e => e.ClientId == request.ClientId && e.RootSessionId == request.RootSessionId);

                if (session?.BlockedTo != null && session.BlockedTo.Value > DateTime.UtcNow)
                {
                    var blockedTime = DateTime.UtcNow - session.BlockedTo.Value;

                    if (blockedTime.TotalSeconds >= 1)
                    {
                        record.HasPinIssue = true;
                        var response = new CheckPinGrpcResponse()
                        {
                            IsValid = false,
                            BlockedTime = blockedTime,
                            Attempts = 0,
                            TerminateSession = false
                        };
                        
                        _logger.LogInformation("FAIL Check pin for client: {clientId}. Response: {responseJson}", 
                            request.ClientId, response.ToJson());
                        return response;
                    }
                }

                var isValid = record.CheckPin(request.Pin);

                if (!isValid)
                {
                    if (session == null)
                    {
                        session = new PinRecordSessionIssue()
                        {
                            ClientId = record.ClientId,
                            RootSessionId = request.RootSessionId,
                            CurrentFailAttempts = 0,
                            TotalFailAttempts = 0,
                            BlockedTo = null
                        };
                        ctx.PinRecordSessionIssues.Add(session);
                    }

                    session.CurrentFailAttempts++;
                    session.TotalFailAttempts++;
                    record.HasPinIssue = true;

                    if (session.TotalFailAttempts >= Program.Settings.CountFailAttemptsToTerminate)
                    {
                        record.HasPinIssue = true;
                        var response = new CheckPinGrpcResponse()
                        {
                            IsValid = false,
                            BlockedTime = TimeSpan.Zero,
                            Attempts = 0,
                            TerminateSession = true,
                            BlockPin = false
                        };
                        _logger.LogInformation("FAIL Check pin for client: {clientId}. Response: {responseJson}", 
                            request.ClientId, response.ToJson());
                        return response;
                    }
                    else if (session.CurrentFailAttempts >= Program.Settings.CountFailAttemptsToBlock)
                    {
                        record.HasPinIssue = true;
                        session.BlockedTo = DateTime.UtcNow.AddSeconds(Program.Settings.BlockTimeInSec);
                        session.CurrentFailAttempts = 0;
                        var response = new CheckPinGrpcResponse()
                        {
                            IsValid = false,
                            BlockedTime = TimeSpan.FromSeconds(Program.Settings.BlockTimeInSec),
                            Attempts = 0,
                            TerminateSession = false,
                            BlockPin = true
                        };
                        _logger.LogInformation("FAIL Check pin for client: {clientId}. Response: {responseJson}", 
                            request.ClientId, response.ToJson());
                        return response;
                    }
                    else
                    {
                        var attempts = Program.Settings.CountFailAttemptsToBlock - session.CurrentFailAttempts;
                        if (attempts < 1)
                            attempts = 1;
                        
                        record.HasPinIssue = true;
                        
                        var response = new CheckPinGrpcResponse()
                        {
                            IsValid = false,
                            BlockedTime = TimeSpan.Zero,
                            Attempts = attempts,
                            TerminateSession = false,
                            BlockPin = false
                        };
                        _logger.LogInformation("FAIL Check pin for client: {clientId}. Response: {responseJson}", 
                            request.ClientId, response.ToJson());
                        return response;
                    }
                }

                if (session != null)
                {
                    session.CurrentFailAttempts = 0;
                    session.TotalFailAttempts = 0;
                }

                record.HasPinIssue = false;

                return new CheckPinGrpcResponse()
                {
                    IsValid = true,
                    BlockedTime = TimeSpan.Zero,
                    Attempts = 0,
                    TerminateSession = false,
                    BlockPin = false
                };
            }
            finally
            {
                await _writer.InsertOrReplaceAsync(PinRecordNoSqlEntity.Create(record));
                await ctx.SaveChangesAsync();
            }
            
        }

        public async Task<IsPinInitedResponse> IsPinInitedAsync(IsPinInitedRequest request)
        {
            PinRecord? record = null;

            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
                record = await ctx.PinRecords.FirstOrDefaultAsync(e => e.ClientId == request.ClientId);
                if (record == null)
                {
                    record = new PinRecord()
                    {
                        ClientId = request.ClientId,
                        Hash = string.Empty,
                        Salt = Guid.NewGuid().ToString("N"),
                        HasPinIssue = false,
                        IsInited = false
                    };
                    ctx.PinRecords.Add(record);
                    await ctx.SaveChangesAsync();
                }
            }
            await _writer.InsertOrReplaceAsync(PinRecordNoSqlEntity.Create(record));

            return new IsPinInitedResponse()
            {
                IsInited = record.IsInited
            };
        }
    }
}