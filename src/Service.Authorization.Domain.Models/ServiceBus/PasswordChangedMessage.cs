using System;
using System.Runtime.Serialization;

namespace Service.Authorization.Domain.Models.ServiceBus
{
    [DataContract]
    public class PasswordChangedMessage
    {
        public const string TopicName = "jet-wallet-client-change-password";
        
        [DataMember(Order = 1)]
        public string TraderId { get; set; }

        [DataMember(Order = 2)]
        public string Brand { get; set; }
        
        [DataMember(Order = 3)]
        public DateTime DatePublish { get; set; }
    }
}