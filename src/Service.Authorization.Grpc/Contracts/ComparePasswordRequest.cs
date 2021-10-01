using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class ComparePasswordRequest
    {
        [DataMember(Order = 1)]
        public string TraderId { get; set; }

        [DataMember(Order = 2)]
        public string Hash { get; set; }
        
        [DataMember(Order = 3)]
        public string Salt { get; set; }

        [DataMember(Order = 4)]
        public string Brand { get; set; }

        public static ComparePasswordRequest Create(string traderId, string hash, string salt, string brand)
        {
            return new ComparePasswordRequest
            {
                TraderId = traderId,
                Hash = hash,
                Salt = salt,
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