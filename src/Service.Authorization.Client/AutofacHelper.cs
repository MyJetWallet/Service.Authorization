using Autofac;
using Microsoft.Extensions.DependencyInjection;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using Service.Authorization.Client.Http;
using Service.Authorization.Domain.Models.NoSql;
using Service.Authorization.Grpc;
using Service.Wallet.Api.Authentication;

// ReSharper disable UnusedMember.Global

namespace Service.Authorization.Client
{
    public static class AutofacHelper
    {
        public static void RegisterAuthorizationClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new AuthorizationClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetAuthorizationService()).As<IAuthorizationService>().SingleInstance();
        }

        public static void AddAuthenticationJetWallet(this IServiceCollection services)
        {
            services
                .AddAuthentication(o => { o.DefaultScheme = "Bearer"; })
                .AddScheme<WalletAuthenticationOptions, WalletAuthHandler>("Bearer", o => { });
        }

        public static void RegisterAuthorizationSessionCache(this ContainerBuilder builder, IMyNoSqlSubscriber myNoSqlSubscriber)
        {
            var reader = new MyNoSqlReadRepository<SpotSessionNoSql>(myNoSqlSubscriber, SpotSessionNoSql.TableName);
            builder.RegisterInstance(reader).As<IMyNoSqlServerDataReader<SpotSessionNoSql>>().SingleInstance();
        }
    }
}
