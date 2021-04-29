using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.Authorization.Grpc;

namespace Service.Authorization.Client
{
    [UsedImplicitly]
    public class AuthorizationClientFactory
    {
        private readonly CallInvoker _channel;

        public AuthorizationClientFactory(string authorizationGrpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(authorizationGrpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public IAuthorizationService GetAuthorizationService() => _channel.CreateGrpcService<IAuthorizationService>();
    }
}
