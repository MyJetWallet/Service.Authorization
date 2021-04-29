using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Models
{
    [DataContract]
    public class KillRootSessionRequest
    {
        [DataMember(Order = 1)]
        public string Token { get; set; }

        [DataMember(Order = 2)]
        public string Reason { get; set; }
    }
}