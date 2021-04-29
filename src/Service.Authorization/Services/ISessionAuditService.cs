using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.Authorization.DataBase;
using Service.Authorization.Domain.Models;

namespace Service.Authorization.Services
{
    public interface ISessionAuditService
    {
        Task NewSessionAudit(JetWalletToken baseToken, JetWalletToken newToken, string userAgent, string ip);
        Task RefreshSessionAudit(JetWalletToken prevToken, JetWalletToken newToken, string userAgent, string ip);
        Task KillSessionAudit(string sessionRootId, string sessionId, string clientId, string reason, string userAgent, string ip);
        Task<List<Domain.Models.SessionAudit>> GetActiveSessions(string clientId);
    }

    public class SessionAuditServiceService : ISessionAuditService
    {
        private readonly DatabaseContextFactory _databaseContextFactory;

        public SessionAuditServiceService(DatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task NewSessionAudit(JetWalletToken baseToken, JetWalletToken newToken, string userAgent, string ip)
        {
            await using var ctx = _databaseContextFactory.Create();

            var entity = new SessionAudit()
            {
                SessionRootId = newToken.SessionRootId,
                SessionId = newToken.SessionRootId,
                BaseSessionId = null,
                BrokerId = newToken.BrokerId,
                BrandId = newToken.BrandId,
                ClientId = newToken.ClientId(),
                WalletId = newToken.WalletId,
                CreateTime = DateTime.UtcNow,
                Expires = newToken.Expires,
                UserAgent = userAgent,
                Ip = ip
            };

            await ctx.UpsetAsync(entity);
        }

        public async Task RefreshSessionAudit(JetWalletToken prevToken, JetWalletToken newToken, string userAgent, string ip)
        {
            await using var ctx = _databaseContextFactory.Create();

            var entity = new SessionAudit()
            {
                SessionRootId = newToken.SessionRootId,
                SessionId = newToken.SessionRootId,
                BaseSessionId = prevToken.SessionId,
                BrokerId = newToken.BrokerId,
                BrandId = newToken.BrandId,
                ClientId = newToken.ClientId(),
                WalletId = newToken.WalletId,
                CreateTime = DateTime.UtcNow,
                Expires = newToken.Expires,
                UserAgent = userAgent,
                Ip = ip
            };

            await ctx.UpsetAsync(entity);
        }

        public async Task KillSessionAudit(string sessionRootId, string sessionId, string clientId, string reason, string userAgent, string ip)
        {
            await using var ctx = _databaseContextFactory.Create();


            var entity = new KillSessionAuditEntity()
            {
                SessionId = sessionId,
                SessionRootId = sessionRootId,
                KillTime = DateTime.UtcNow,
                Ip = ip,
                Reason = reason,
                UserAgent = userAgent
            };

            await ctx.UpsetAsync(entity);
        }

        public async Task<List<SessionAudit>> GetActiveSessions(string clientId)
        {
            await using var ctx = _databaseContextFactory.Create();

            var sql = "select s.* from \"authorization\".sessions s left join \"authorization\".kills k on s.\"SessionRootId\" = k.\"SessionRootId\" where k.\"SessionRootId\" is null and s.\"Expires\" < CURRENT_TIMESTAMP and s.\"ClientId\" = '{0}'";

            var data = await ctx.Sessions.FromSqlRaw(sql, clientId).ToListAsync();

            return data;
        }
    }
}