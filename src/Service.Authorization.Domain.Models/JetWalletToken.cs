using System;
using System.Runtime.Serialization;
using SimpleTrading.TokensManager.Tokens;

namespace Service.Authorization.Domain.Models
{
    [DataContract]
    public class JetWalletToken : ITokenBase
    {
        [DataMember(Order = 1)] public string Id { get; set; }
        [DataMember(Order = 2)] public DateTime Expires { get; set; }

        [DataMember(Order = 100)] public string SessionRootId { get; set; }
        [DataMember(Order = 101)] public string SessionId { get; set; }

        [DataMember(Order = 110)] public string BrokerId { get; set; }
        [DataMember(Order = 111)] public string BrandId { get; set; }
        [DataMember(Order = 112)] public string WalletId { get; set; }

        public string ClientId() => Id;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id) &&
                   !string.IsNullOrEmpty(SessionId) &&
                   !string.IsNullOrEmpty(SessionRootId) &&
                   !string.IsNullOrEmpty(BrandId) &&
                   !string.IsNullOrEmpty(BrokerId) &&
                   !string.IsNullOrEmpty(WalletId) &&
                   DateTime.UtcNow < Expires;

        }
    }
}