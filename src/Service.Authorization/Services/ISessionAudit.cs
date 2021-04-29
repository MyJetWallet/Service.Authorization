using System;
using System.Threading.Tasks;
using Service.Authorization.DataBase;
using Service.Authorization.Domain.Models;

namespace Service.Authorization.Services
{
    public interface ISessionAudit
    {
        Task NewSessionAudit(JetWalletToken baseToken, JetWalletToken newToken, string userAgent, string ip);
        Task RefreshSessionAudit(JetWalletToken prevToken, JetWalletToken newToken, string userAgent, string ip);
        Task KillSessionAudit(string sessionRootId, string sessionId, string clientId, string reason, string userAgent, string ip);
    }

    public class SessionAudit : ISessionAudit
    {
        private readonly DatabaseContextFactory _databaseContextFactory;

        public SessionAudit(DatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task NewSessionAudit(JetWalletToken baseToken, JetWalletToken newToken, string userAgent, string ip)
        {
            await using var ctx = _databaseContextFactory.Create();

            var entity = new SessionAuditEntity()
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

            var entity = new SessionAuditEntity()
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
    }
}