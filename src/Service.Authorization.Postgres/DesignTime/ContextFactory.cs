using MyJetWallet.Sdk.Postgres;
using Service.Authorization.Postgres;

namespace Service.InternalTransfer.Postgres.DesignTime
{
    public class ContextFactory : MyDesignTimeContextFactory<DatabaseContext>
    {
        public ContextFactory() : base(options => new DatabaseContext(options))
        {
        }
    }
}