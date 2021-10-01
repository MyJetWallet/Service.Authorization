using System.ServiceModel;
using System.Threading.Tasks;
using Service.Authorization.Grpc.Contracts;

namespace Service.Authorization.Grpc
{
    [ServiceContract]
    public interface IAuthService
    {
        [OperationContract]
        ValueTask<AuthenticateGrpcResponse> AuthenticateAsync(AuthenticateGrpcRequest request);

        [OperationContract]
        ValueTask<ExistsGrpcResponse> ExistsAsync(ExistsGrpcRequest request);
        
        [OperationContract]
        ValueTask<GetIdGrpcResponse> GetIdByEmailAsync(GetIdGrpcRequest request);
        
        [OperationContract]
        ValueTask<GetIdsByEmailGrpcResponse> GetIdsByEmailAsync(GetIdsByEmailGrpcRequest request);

        [OperationContract]
        ValueTask<GetEmailByIdGrpcResponse> GetEmailByIdAsync(GetEmailByIdGrpcRequest request);
        
        [OperationContract]
        ValueTask<ChangePasswordGrpcResponse> ChangePasswordAsync(ChangePasswordGrpcContract request);

        [OperationContract]
        ValueTask<ComparePasswordResponse> ComparePasswordAsync(ComparePasswordRequest request);

        [OperationContract]
        ValueTask ClearCacheAsync(ClearCacheRequest request);
    }
}