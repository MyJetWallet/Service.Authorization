using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class ComparePasswordRequest
    {
        [DataMember(Order = 1)]
        public string TraderId { get; set; }

        [LogMasked]
        [DataMember(Order = 2)]
        public string Password { get; set; }
        
        [DataMember(Order = 3)]
        public string Brand { get; set; }

        public static ComparePasswordRequest Create(string traderId, string password, string brand)
        {
            return new ComparePasswordRequest
            {
                TraderId = traderId,
                Password = password,
                Brand = brand
            };
        }
    }

    [DataContract]
    public class ComparePasswordResponse
    {
        [DataMember(Order = 1)]
        public bool Ok { get; set; }
    }
}