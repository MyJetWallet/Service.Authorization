using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class AuthenticateGrpcRequest
    {
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [DataMember(Order = 1)] public string Email { get; set; }
        
        [DataMember(Order = 2)] public string Hash { get; set; }
        
        [DataMember(Order = 3)] public string Salt { get; set; }

        [DataMember(Order = 4)] public string Brand { get; set; }

        [DataMember(Order = 5)] public string Ip { get; set; }

        [DataMember(Order = 6)] public string UserAgent { get; set; }

        [DataMember(Order = 7)] public string LanguageId { get; set; }

        [DataMember(Order = 8)] public string Location { get; set; }

        public static AuthenticateGrpcRequest Create(string email, string hash, string salt, string brand, string ip,
            string userAgent, string languageId, string location)
        {
            return new AuthenticateGrpcRequest
            {
                Email = email,
                Hash = hash,
                Salt = salt,
                Brand = brand,
                Ip = ip,
                UserAgent = userAgent,
                LanguageId = languageId,
                Location = location
            };
        }
    }

    [DataContract]
    public class AuthenticateGrpcResponse
    {
        [DataMember(Order = 1)] public string TraderId { get; set; }
    }
}