using System;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Service.Authorization.Domain.Models.ServiceBus;
using Service.Authorization.NoSql;
using Service.Authorization.Postgres;
using Service.Authorization.Services;
using Service.ClientBlocker.Client;

namespace Service.Authorization.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            var spotServiceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), Program.LogFactory);

            builder
                .RegisterInstance(MyNoSqlAuthCacheFactory.CreateAuthCacheNoSqlWriter(
                    Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                    Program.EncodingKey,
                    Program.EncodingInitVector))
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterClientBlockerClient(Program.Settings.ClientBlockerGrpcService);
            
            builder
                .RegisterInstance(noSqlClient.CreateAuthCacheMyNoSqlReader(Program.EncodingKey,
                    Program.EncodingInitVector))
                .AsSelf()
                .SingleInstance();

            builder.RegisterMyServiceBusPublisher<ClientAuthenticationMessage>(spotServiceBusClient,
                ClientAuthenticationMessage.TopicName, false);
            
            builder.RegisterMyServiceBusPublisher<PasswordChangedMessage>(spotServiceBusClient,
                PasswordChangedMessage.TopicName, false);
            
            builder.RegisterType<AuthenticationCredentialsRepository>().AsSelf().SingleInstance();
            builder.RegisterType<AuthLogRepository>().AsSelf().SingleInstance();
            builder.RegisterType<AuthLogQueue>().AsSelf().SingleInstance();
        }
    }
}