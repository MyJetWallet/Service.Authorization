using Autofac;
using Service.Authorization.Grpc;

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
    }
}
