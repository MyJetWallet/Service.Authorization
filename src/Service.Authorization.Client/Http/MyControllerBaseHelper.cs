﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain;
// ReSharper disable UnusedMember.Global

namespace Service.Authorization.Client.Http
{
    public static class MyControllerBaseHelper
    {
        public static string GetClientId(this ControllerBase controller)
        {
            var clientIdClaim = controller?.HttpContext?.User?.Claims.FirstOrDefault(e => e.Type == WalletAuthHandler.ClientIdClaim);
            var clientId = clientIdClaim?.Value;

            if (string.IsNullOrEmpty(clientId))
            {
                throw new Exception("Cannot extract ClientId");
            }

            return clientId;
        }

        public static string GetWalletId(this ControllerBase controller)
        {
            var claim = controller?.HttpContext?.User?.Claims.FirstOrDefault(e => e.Type == WalletAuthHandler.WalletIdClaim);
            var result = claim?.Value;

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Cannot extract WalletId");
            }

            return result;
        }

        public static string GetBrokerId(this ControllerBase controller)
        {
            var claim = controller?.HttpContext?.User?.Claims.FirstOrDefault(e => e.Type == WalletAuthHandler.BrokerIdClaim);
            var result = claim?.Value;

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Cannot extract BrokerId");
            }

            return result;
        }

        public static string GetBrandId(this ControllerBase controller)
        {
            var claim = controller?.HttpContext?.User?.Claims.FirstOrDefault(e => e.Type == WalletAuthHandler.BrandIdClaim);
            var result = claim?.Value;

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Cannot extract BrandId");
            }

            return result;
        }

        public static (string, string) GetSessionId(this ControllerBase controller)
        {
            var claim = controller?.HttpContext?.User?.Claims.FirstOrDefault(e => e.Type == WalletAuthHandler.SessionIdClaim);
            var sessionId = claim?.Value;

            claim = controller?.HttpContext?.User?.Claims.FirstOrDefault(e => e.Type == WalletAuthHandler.SessionRootIdClaim);
            var sessionRootId = claim?.Value;

            if (string.IsNullOrEmpty(sessionRootId))
            {
                throw new Exception("Cannot extract SessionRootId");
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new Exception("Cannot extract SessionId");
            }

            return (sessionRootId, sessionId);
        }

        public static string GetSignature(this ControllerBase controller)
        {
            var claim = controller?.HttpContext?.User?.Claims.FirstOrDefault(e => e.Type == WalletAuthHandler.SignatureClaim);
            var result = claim?.Value;

            return result;
        }

        public static JetWalletIdentity GetWalletIdentity(this ControllerBase controller)
        {
            var wallet = new JetWalletIdentity(controller.GetBrokerId(), controller.GetBrandId(), controller.GetClientId(), controller.GetWalletId());

            return wallet;
        }
    }
}