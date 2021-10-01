using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class TraderBrandGrpcModel
    {
        [DataMember(Order = 1)]
        public string TraderId { get; set; }

        [DataMember(Order = 2)]
        public string Brand { get; set; }
    }
}