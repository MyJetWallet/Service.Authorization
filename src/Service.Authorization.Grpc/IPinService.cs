using System.ServiceModel;
using System.Threading.Tasks;
using Service.Authorization.Domain.Models;
using Service.Authorization.Grpc.Contracts.PinDto;

namespace Service.Authorization.Grpc;

[ServiceContract]
public interface IPinService
{
    [OperationContract]
    Task SetupPinAsync(SetupPinGrpcRequest request);

    [OperationContract]
    Task RemovePinAsync(RemovePinGrpcRequest request);
    
    [OperationContract]
    Task<CheckPinGrpcResponse> CheckPinAsync(CheckPinGrpcRequest request);
    
    [OperationContract]
    Task<IsPinInitedResponse> IsPinInitedAsync(IsPinInitedRequest request);
}