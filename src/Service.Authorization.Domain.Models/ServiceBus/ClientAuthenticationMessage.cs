using System.Runtime.Serialization;

namespace Service.Authorization.Domain.Models.ServiceBus
{
    [DataContract]
    public class ClientAuthenticationMessage
    {
        public const string TopicName = "jet-wallet-client-authentication";

        [DataMember(Order = 1)]
        public string TraderId { get; set; }

        [DataMember(Order = 2)]
        public string Brand { get; set; }

        [DataMember(Order = 3)]
        public string Ip { get; set; }

        [DataMember(Order = 4)]
        public string UserAgent { get; set; }

        [DataMember(Order = 5)]
        public string LanguageId { get; set; }
    }
}