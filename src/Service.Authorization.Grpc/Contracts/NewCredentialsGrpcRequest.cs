using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class NewCredentialsGrpcRequest
    {
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [DataMember(Order = 1)] public string Email { get; set; }

        [LogMasked]
        [DataMember(Order = 2)] public string Password { get; set; }
        
        [DataMember(Order = 3)] public string AffId { get; set; }
        
        [DataMember(Order = 4)] public string CxdToken { get; set; }

        public static NewCredentialsGrpcRequest Create(string email, string password)
        {
            return new NewCredentialsGrpcRequest
            {
                Email = email,
                Password = password
            };
        }
    }

    [DataContract]
    public class NewCredentialsGrpcResponse
    {
        [DataMember(Order = 1)] public string TraderId { get; set; }

        [DataMember(Order = 2)] public ResponseStatuses Status { get; set; }

        public static NewCredentialsGrpcResponse Create(string traderId, ResponseStatuses status)
        {
            return new NewCredentialsGrpcResponse
            {
                TraderId = traderId,
                Status = status
            };
        }
    }
}