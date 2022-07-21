using System.ServiceModel;
using System.Threading.Tasks;
using Service.Authorization.Domain.Models;
using Service.Authorization.Grpc.Contracts.PinDto;

namespace Service.Authorization.Grpc;

[ServiceContract]
public interface IPinService
{
    [OperationContract]
    Task SetupPinAsync(PinRecord request);
    [OperationContract]
    Task<CheckPinGrpcResponse> CheckPinAsync(CheckPinGrpcRequest request);
}