using System.Runtime.Serialization;
using Google.Protobuf.WellKnownTypes;

namespace Service.Authorization.Grpc.Models
{
    [DataContract]
    public class AuthorizationRequest
    {
        [DataMember(Order = 1)]
        public string Token { get; set; }

        [DataMember(Order = 2)]
        public string BrokerId { get; set; }

        [DataMember(Order = 3)]
        public string BrandId { get; set; }

        [DataMember(Order = 4)]
        public string WalletId { get; set; }


        /// <summary>
        /// https://8gwifi.org/RSAFunctionality?rsasignverifyfunctions=rsasignverifyfunctions&keysize=2048
        /// </summary>
        [DataMember(Order = 5)]
        public string PublicKeyPem { get; set; }

        [DataMember(Order = 6)]
        public string UserAgent { get; set; }

        [DataMember(Order = 7)]
        public string Ip { get; set; }
    }
}