using System;

namespace Service.Authorization.DataBase
{
    public class KillSessionAuditEntity
    {
        public string SessionRootId { get; set; }
        public string SessionId { get; set; }
        public DateTime KillTime { get; set; }
        public string UserAgent { get; set; }
        public string Reason { get; set; }
        public string Ip { get; set; }
    }
}