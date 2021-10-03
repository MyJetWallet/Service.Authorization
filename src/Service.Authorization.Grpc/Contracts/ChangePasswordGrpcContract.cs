using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class ChangePasswordGrpcContract
    {
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [DataMember(Order = 1)]
        public string Email { get; set; }
        
        [LogMasked]
        [DataMember(Order = 2)]
        public string Hash { get; set; }

        [DataMember(Order = 3)] 
        public string Salt { get; set; }
        
        [DataMember(Order = 4)]
        public string Brand { get; set; }

        public static ChangePasswordGrpcContract Create(string email, string hash, string salt, string brand)
        {
            return new ChangePasswordGrpcContract
            {
                Email = email,
                Hash = hash,
                Salt = salt,
                Brand = brand
            };
        }
    }
    
    [DataContract]
    public class ChangePasswordGrpcResponse
    {

    }
}