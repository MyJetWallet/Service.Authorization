using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class GetIdGrpcRequest
    {
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [DataMember(Order = 1)]
        public string Email { get; set; }
        
        [DataMember(Order = 2)]
        public string Brand { get; set; }

        public static GetIdGrpcRequest Create(string email, string brand)
        {
            return new GetIdGrpcRequest
            {
                Email = email,
                Brand = brand
            };
        }
    }
    
    
    [DataContract]
    public class GetIdGrpcResponse
    {
        [DataMember(Order = 1)]
        public string TraderId { get; set; }
    }
}