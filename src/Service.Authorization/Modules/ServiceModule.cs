using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using Service.Authorization.Domain.Models.ServiceBus;
using Service.Authorization.NoSql;
using Service.Authorization.Services;

namespace Service.Authorization.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            var spotServiceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), ApplicationEnvironment.HostName, Program.LogFactory);

            builder
                .RegisterInstance(MyNoSqlAuthCacheFactory.CreateAuthCacheNoSqlWriter(
                    Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                    Program.EncodingKey,
                    Program.EncodingInitVector))
                .AsSelf()
                .SingleInstance();
            
            builder
                .RegisterInstance(noSqlClient.CreateAuthCacheMyNoSqlReader(Program.EncodingKey,
                    Program.EncodingInitVector))
                .AsSelf()
                .SingleInstance();

            builder.RegisterMyServiceBusPublisher<ClientAuthenticationMessage>(spotServiceBusClient,
                ClientAuthenticationMessage.TopicName, false);

            builder.RegisterType<AuthenticationCredentialsRepository>().AsSelf().SingleInstance();
            builder.RegisterType<AuthLogRepository>().AsSelf().SingleInstance();
            builder.RegisterType<AuthLogQueue>().AsSelf().SingleInstance();
        }
    }
}