using System;
using System.Threading.Tasks;
using Service.Authorization.DataBase;
using Service.Authorization.Domain.Models;

namespace Service.Authorization.Services
{
    public interface ISessionAudit
    {
        Task NewSessionAudit(JetWalletToken baseToken, JetWalletToken newToken, string userAgent);
        Task RefreshSessionAudit(JetWalletToken prevToken, JetWalletToken newToken, string userAgent);
        Task KillSessionAudit(JetWalletToken token);
    }

    public class SessionAudit : ISessionAudit
    {
        private readonly DatabaseContextFactory _databaseContextFactory;

        public SessionAudit(DatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task NewSessionAudit(JetWalletToken baseToken, JetWalletToken newToken, string userAgent)
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
                UserAgent = userAgent
            };

            await ctx.UpsetAsync(entity);
        }

        public async Task RefreshSessionAudit(JetWalletToken prevToken, JetWalletToken newToken, string userAgent)
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
                UserAgent = userAgent
            };

            await ctx.UpsetAsync(entity);
        }

        public async Task KillSessionAudit(JetWalletToken token)
        {
            await using var ctx = _databaseContextFactory.Create();


            var entity = new KillSessionAuditEntity()
            {
                SessionId = token.SessionId,
                SessionRootId = token.SessionRootId,
                KillTime = DateTime.UtcNow
            };

            await ctx.UpsetAsync(entity);
        }
    }
}