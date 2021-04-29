using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Models
{
    [DataContract]
    public class KillRootSessionRequest
    {
        [DataMember(Order = 1)]
        public string SessionRootId { get; set; }

        [DataMember(Order = 2)]
        public string ClientId { get; set; }

        [DataMember(Order = 3)]
        public string Reason { get; set; }

        [DataMember(Order = 4)]
        public string UserAgent { get; set; }

        [DataMember(Order = 5)]
        public string Ip { get; set; }

        [DataMember(Order = 6)]
        public string SessionId { get; set; }
    }
}