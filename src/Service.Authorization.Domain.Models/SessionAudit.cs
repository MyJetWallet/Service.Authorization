using System;

namespace Service.Authorization.Domain.Models
{
    public class SessionAudit
    {
        public string SessionId { get; set; }
        public string SessionRootId { get; set; }
        
        public string ClientId { get; set; }
        public DateTime Expires { get; set; }
        
        public string BrokerId { get; set; }
        public string BrandId { get; set; }
        public string WalletId { get; set; }

        public string BaseSessionId { get; set; }
        public DateTime CreateTime { get; set; }
        public string UserAgent { get; set; }
        public string Ip { get; set; }
    }
}