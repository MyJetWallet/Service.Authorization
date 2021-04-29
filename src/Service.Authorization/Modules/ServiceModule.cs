using Autofac;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using Service.Authorization.DataBase;
using Service.Authorization.Domain.Models.NoSql;
using Service.Authorization.Services;
using Service.ClientWallets.Client;
using Service.Registration.Client;

namespace Service.Authorization.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = new MyNoSqlTcpClient(
                Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort),
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder.RegisterInstance(myNoSqlClient).AsSelf().SingleInstance();

            RegisterMyNoSqlWriter<SpotSessionNoSql>(builder, SpotSessionNoSql.TableName);

            builder.RegisterClientRegistrationClient(myNoSqlClient, Program.Settings.RegistrationGrpcServiceUrl);
            builder.RegisterClientWalletsClients(myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);

            builder.RegisterType<DatabaseContextFactory>().AsSelf().SingleInstance();

            builder.RegisterType<SessionAuditServiceService>().As<ISessionAuditService>().SingleInstance();
        }


        private void RegisterMyNoSqlWriter<TEntity>(ContainerBuilder builder, string table)
            where TEntity : IMyNoSqlDbEntity, new()
        {
            builder.Register(ctx => new MyNoSqlServer.DataWriter.MyNoSqlServerDataWriter<TEntity>(
                    Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), table, true))
                .As<IMyNoSqlServerDataWriter<TEntity>>()
                .SingleInstance();
        }
    }
}