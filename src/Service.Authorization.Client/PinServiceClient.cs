using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using Service.Authorization.Domain.Models;
using Service.Authorization.Grpc;
using Service.Authorization.Grpc.Contracts.PinDto;
using Service.Authorization.NoSql;

namespace Service.Authorization.Client;

public class PinServiceClient: IPinService
{
    private readonly IPinService _service;
    private readonly IMyNoSqlServerDataReader<PinRecordNoSqlEntity> _reader;

    public PinServiceClient(IPinService service, IMyNoSqlServerDataReader<PinRecordNoSqlEntity> reader)
    {
        _service = service;
        _reader = reader;
    }

    public Task SetupPinAsync(SetupPinGrpcRequest request)
    {
        return _service.SetupPinAsync(request);
    }

    public Task RemovePinAsync(RemovePinGrpcRequest request)
    {
        return _service.RemovePinAsync(request);
    }

    public async Task<CheckPinGrpcResponse> CheckPinAsync(CheckPinGrpcRequest request)
    {
        var record = _reader.Get(
            PinRecordNoSqlEntity.GeneratePartitionKey(request.ClientId),
            PinRecordNoSqlEntity.GenerateRowKey());

        if (record != null && !record.Pin.HasPinIssue && record.Pin.CheckPin(request.Pin))
        {
            return new CheckPinGrpcResponse
            {
                IsValid = record.Pin.CheckPin(request.Pin)
            };
        }
        
        return await _service.CheckPinAsync(request);
    }
}