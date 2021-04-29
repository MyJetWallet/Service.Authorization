using System;
using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Models
{
    [DataContract]
    public class RefreshSessionRequest
    {
        [DataMember(Order = 1)]
        public string Token { get; set; }

        [DataMember(Order = 2)]
        public DateTime RequestTimestamp { get; set; }

        [DataMember(Order = 3)]
        public string NewWalletId { get; set; }

        /// <summary>
        /// Signature for request with client private key associated with root session. Algorithm:
        ///  * privateKey = Rsa.GeneratePrivateKey(2048)
        ///  * text = "{Token}_{RequestTimestamp:yyyy-MM-ddTHH:mm:ss}_{NewWalletId}"
        ///  * buf[] = Utf8.GetBytes(text)
        ///  * hash[] = Sha256(buf)
        ///  * signature = RSA.Pkcs1.Sign(hash[], privateKey)
        /// </summary>
        [DataMember(Order = 4)]
        public string SignatureBase64 { get; set; }

        [DataMember(Order = 6)]
        public string UserAgent { get; set; }
    }

}