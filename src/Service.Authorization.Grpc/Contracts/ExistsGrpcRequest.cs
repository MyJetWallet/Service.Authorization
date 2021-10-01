using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class ExistsGrpcRequest
    {
        [DataMember(Order = 1)]
        public string TraderId { get; set; }
        
        [DataMember(Order = 2)]
        public string Brand { get; set; }

        public static ExistsGrpcRequest Create(string traderId, string brand)
        {
            return new ExistsGrpcRequest
            {
                TraderId = traderId,
                Brand = brand
            };
        }
    }
    
    [DataContract]
    public class ExistsGrpcResponse
    {
        [DataMember(Order = 1)]
        public bool Yes { get; set; }
    }

}