﻿using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Authorization.Grpc;

namespace Service.Authorization.Client
{
    [UsedImplicitly]
    public class AuthorizationClientFactory: MyGrpcClientFactory
    {
        public AuthorizationClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IAuthService GetAuthService() => CreateGrpcService<IAuthService>();
    }
}
