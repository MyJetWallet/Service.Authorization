using System.ServiceModel;
using System.Threading.Tasks;
using Service.Authorization.Domain.Models;
using Service.Authorization.Grpc.Models;

namespace Service.Authorization.Grpc
{
    [ServiceContract]
    public interface IAuthorizationService
    {
        [OperationContract]
        Task<AuthorizationResponse> AuthorizationAsync(AuthorizationRequest request);

        [OperationContract]
        Task KillRootSessionAsync(KillRootSessionRequest request);

        [OperationContract]
        Task<AuthorizationResponse> RefreshSessionAsync(RefreshSessionRequest request);

        [OperationContract]
        Task<ListResponse<SessionAudit>> GetActiveSessionsAsync(GetActiveSessionsRequest request);
    }
}