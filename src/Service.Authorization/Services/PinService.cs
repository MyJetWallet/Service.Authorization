using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
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

        public async Task SetupPinAsync(PinRecord request)
        {
            _logger.LogInformation("Setup pin for client: {clientId}", request.ClientId);
            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
                await ctx.PinRecords.Upsert(request).On(e => e.ClientId).RunAsync();
            }
            await _writer.InsertOrReplaceAsync(PinRecordNoSqlEntity.Create(request));
        }

        public async Task<CheckPinGrpcResponse> CheckPinAsync(CheckPinGrpcRequest request)
        {
            _logger.LogInformation("Setup pin for client: {clientId}", request.ClientId);
        
            PinRecord record = null;
            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
                record = await ctx.PinRecords.AsNoTracking().FirstOrDefaultAsync(e => e.ClientId == request.ClientId);
            }
            if (record == null)
                return new CheckPinGrpcResponse() {IsValid = false};
            
            await _writer.InsertOrReplaceAsync(PinRecordNoSqlEntity.Create(record));

            var resp = new CheckPinGrpcResponse
            {
                IsValid = record.CheckPin(request.Pin)
            };
            return resp;
        }
    }
}