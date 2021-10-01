using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Authorization.Domain.Models.ServiceBus;
using Service.Authorization.Grpc;
using Service.Authorization.Postgres;

// ReSharper disable UnusedMember.Global

namespace Service.Authorization.Client
{
    public static class AutofacHelper
    {
        public static void RegisterAuthorizationClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new AuthorizationClientFactory(grpcServiceUrl);
            builder.RegisterInstance(factory.GetAuthService()).As<IAuthService>().SingleInstance();
        }
        
        public static void RegisterClientAuthenticationSubscriber(this ContainerBuilder builder, MyServiceBusTcpClient serviceBusClient, string queue)
        {
            builder.RegisterMyServiceBusSubscriberBatch<ClientAuthenticationMessage>(serviceBusClient, ClientAuthenticationMessage.TopicName, queue,
                TopicQueueType.Permanent);
        }

        public static void RegisterAuthCredentialRepository(this ContainerBuilder builder, ILoggerFactory loggerFactory, string postgresConnectionString, byte[] encodingKey, byte[] initVector)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            var options = optionsBuilder
                .UseNpgsql(postgresConnectionString, (contextOptionsBuilder => contextOptionsBuilder.MigrationsHistoryTable("__EFMigrationsHistory_" + DatabaseContext.Schema, DatabaseContext.Schema)))
                .Options;
            
            var repository = new AuthenticationCredentialsRepository(
                loggerFactory.CreateLogger<AuthenticationCredentialsRepository>(),
                optionsBuilder,
                encodingKey,
                initVector
            );
            builder.RegisterInstance(repository).AsSelf().SingleInstance();
        }
    }
}
