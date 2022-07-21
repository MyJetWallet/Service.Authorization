using System;
using System.Buffers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using ProtoBuf.WellKnownTypes;
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
            var pin = request.Pin;
            if (pin == null)
            {
                {
                    _logger.LogError("Receive call SetupPinAsync with empty Pin");
                    throw new Exception("Receive call SetupPinAsync with empty Pin");
                }
            }
            _logger.LogInformation("Setup pin for client: {clientId}, session: {rootSessionId}", 
                pin.ClientId, request.RootSessionId);
            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
                request.Pin.HasPinIssue = false;
                var session = await ctx.PinRecordSessionIssues
                    .FirstOrDefaultAsync(e => e.ClientId == pin.ClientId && e.RootSessionId == request.RootSessionId);

                if (session != null && session.CurrentFailAttempts > 0)
                {
                    session.CurrentFailAttempts = 0;
                    await ctx.SaveChangesAsync();
                }

                pin.HasPinIssue = false;
                    
                await ctx.PinRecords.Upsert(pin).On(e => e.ClientId).RunAsync();
            }
            await _writer.InsertOrReplaceAsync(PinRecordNoSqlEntity.Create(pin));
        }

        public async Task RemovePinAsync(RemovePinGrpcRequest request)
        {
            _logger.LogInformation("Remove pin for client: {clientId}, RootSessionId: {rootSessionId}", 
                request.ClientId, request.RootSessionId);
            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
                var record = await ctx.PinRecords.FirstOrDefaultAsync(e => e.ClientId == request.ClientId);
                if (record != null)
                {
                    ctx.PinRecords.Remove(record);

                    var session = await ctx.PinRecordSessionIssues
                        .FirstOrDefaultAsync(e =>
                            e.ClientId == request.ClientId && e.RootSessionId == request.RootSessionId);

                    if (session != null && session.CurrentFailAttempts > 0)
                    {
                        session.CurrentFailAttempts = 0;
                    }
                    await ctx.SaveChangesAsync();
                }
            }
            await _writer.DeleteAsync(PinRecordNoSqlEntity.GeneratePartitionKey(request.ClientId),
                PinRecordNoSqlEntity.GenerateRowKey());
        }

        public async Task<CheckPinGrpcResponse> CheckPinAsync(CheckPinGrpcRequest request)
        {
            _logger.LogInformation("Setup pin for client: {clientId}", request.ClientId);
        
            PinRecord record = null;

            await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
            try
            {
                record = await ctx.PinRecords.FirstOrDefaultAsync(e => e.ClientId == request.ClientId);
                if (record == null)
                    return new CheckPinGrpcResponse() {IsValid = false};

                var session = await ctx.PinRecordSessionIssues
                    .FirstOrDefaultAsync(
                        e => e.ClientId == request.ClientId && e.RootSessionId == request.RootSessionId);

                if (session?.BlockedTo != null && session.BlockedTo.Value > DateTime.UtcNow)
                {
                    var blockedTime = DateTime.UtcNow - session.BlockedTo.Value;

                    if (blockedTime.TotalSeconds >= 1)
                    {
                        var response = new CheckPinGrpcResponse()
                        {
                            IsValid = false,
                            BlockedTime = blockedTime,
                            Attempts = 0,
                            TerminateSession = false
                        };
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
                        var response = new CheckPinGrpcResponse()
                        {
                            IsValid = false,
                            BlockedTime = TimeSpan.Zero,
                            Attempts = 0,
                            TerminateSession = true,
                            BlockPin = false
                        };
                        return response;
                    }
                    else if (session.CurrentFailAttempts >= Program.Settings.CountFailAttemptsToBlock)
                    {
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
                        return response;
                    }
                    else
                    {
                        var attempts = Program.Settings.CountFailAttemptsToBlock - session.CurrentFailAttempts;
                        if (attempts < 1)
                            attempts = 1;

                        var response = new CheckPinGrpcResponse()
                        {
                            IsValid = false,
                            BlockedTime = TimeSpan.Zero,
                            Attempts = attempts,
                            TerminateSession = false,
                            BlockPin = false
                        };
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
                await ctx.SaveChangesAsync();
            }
        }
    }
}