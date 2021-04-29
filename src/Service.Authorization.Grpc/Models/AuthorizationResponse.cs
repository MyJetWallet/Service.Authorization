using System.Runtime.Serialization;
using Service.Authorization.Domain.Models;

namespace Service.Authorization.Grpc.Models
{
    [DataContract]
    public class AuthorizationResponse 
    {
        [DataMember(Order = 1)]
        public bool Result { get; set; }

        [DataMember(Order = 2)]
        public string Token { get; set; }
    }
}