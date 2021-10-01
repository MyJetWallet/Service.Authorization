using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class ClearCacheRequest
    {
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [DataMember(Order = 1)]
        public string Email { get; set; }
        
        [DataMember(Order = 2)]
        public string Brand { get; set; }

        public static ClearCacheRequest Create(string email, string brand)
        {
            return new ClearCacheRequest
            {
                Email = email,
                Brand = brand
            };
        }
    }
}