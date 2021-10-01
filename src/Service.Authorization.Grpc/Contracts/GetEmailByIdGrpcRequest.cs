using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class GetEmailByIdGrpcRequest
    {
        [DataMember(Order = 1)]
        public string TraderId { get; set; }
        
        [DataMember(Order = 2)]
        public string Brand { get; set; }

        public static GetEmailByIdGrpcRequest Create(string traderId, string brand)
        {
            return new GetEmailByIdGrpcRequest
            {
                TraderId = traderId,
                Brand = brand
            };
        }
    }
    
    
    [DataContract]
    public class GetEmailByIdGrpcResponse
    {
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [DataMember(Order = 1)]
        public string Email { get; set; }
        
        [DataMember(Order = 2)]
        public string Brand { get; set; }
    }
}