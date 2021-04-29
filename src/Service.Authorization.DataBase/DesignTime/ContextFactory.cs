using MyJetWallet.Sdk.Postgres;

namespace Service.Authorization.DataBase.DesignTime
{
    public class ContextFactory : MyDesignTimeContextFactory<DatabaseContext>
    {
        public ContextFactory() : base(options => new DatabaseContext(options))
        {

        }
    }
}