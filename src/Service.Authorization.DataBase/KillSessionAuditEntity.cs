﻿using System;

namespace Service.Authorization.DataBase
{
    public class KillSessionAuditEntity
    {
        public string SessionRootId { get; set; }
        public string SessionId { get; set; }
        public DateTime KillTime { get; set; }
    }
}