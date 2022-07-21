using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.DataReader;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Authorization.Domain.Models.ServiceBus;
using Service.Authorization.Grpc;
using Service.Authorization.NoSql;

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
        
        public static void RegisterClientChangePasswordSubscriber(this ContainerBuilder builder, MyServiceBusTcpClient serviceBusClient, string queue)
        {
            builder.RegisterMyServiceBusSubscriberBatch<PasswordChangedMessage>(serviceBusClient, PasswordChangedMessage.TopicName, queue,
                TopicQueueType.Permanent);
        }


        public static void RegisterAuthCredentialsCache(this ContainerBuilder builder, IMyNoSqlSubscriber noSqlClient, string myNoSqlWriterUrl, byte[] encKey, byte[] initVector)
        {
            builder
                .RegisterInstance(MyNoSqlAuthCacheFactory.CreateAuthCacheNoSqlWriter(
                    ()=>myNoSqlWriterUrl,
                    encKey,
                    initVector))
                .AsSelf()
                .SingleInstance();
            
            builder
                .RegisterInstance(noSqlClient.CreateAuthCacheMyNoSqlReader(encKey, initVector))
                .AsSelf()
                .SingleInstance();
        }

        public static void RegisterPinRecordClientService(this ContainerBuilder builder,
            string authorizationServiceGrpcUrl,
            IMyNoSqlSubscriber noSqlClient)
        {
            var factory = new AuthorizationClientFactory(authorizationServiceGrpcUrl);
            
            builder.RegisterMyNoSqlReader<PinRecordNoSqlEntity>(noSqlClient, PinRecordNoSqlEntity.TableName);
            builder.RegisterType<PinServiceClient>()
                .As<IPinService>()
                .WithParameter("service", factory.GetPinService())
                .SingleInstance()
                .AutoActivate();
        }
        
        public static void RegisterPinRecordClientServiceWithOutCache(this ContainerBuilder builder,
            string authorizationServiceGrpcUrl)
        {
            var factory = new AuthorizationClientFactory(authorizationServiceGrpcUrl);
            
            builder.RegisterInstance(factory.GetPinService())
                .As<IPinService>()
                .SingleInstance();
        }
    }
}
