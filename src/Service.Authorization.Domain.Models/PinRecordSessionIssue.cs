using System;
using System.Runtime.Serialization;

namespace Service.Authorization.Domain.Models
{
    [DataContract]
    public class PinRecordSessionIssue
    {
        [DataMember(Order = 1)]
        public string ClientId { get; set; }
        
        [DataMember(Order = 2)]
        public string RootSessionId { get; set; }
        
        [DataMember(Order = 3)]
        public int TotalFailAttempts { get; set; }
        
        [DataMember(Order = 4)]
        public int CurrentFailAttempts { get; set; }
        
        [DataMember(Order = 5)]
        public bool IsActive { get; set; }
        
        [DataMember(Order = 6)]
        public DateTime? BlockedTo { get; set; }
    }
}