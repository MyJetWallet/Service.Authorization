using System;
using System.Security.Cryptography;
using System.Text;

namespace Service.Authorization.Domain.Models
{
    public static class MyRsa
    {
        public static bool ValidateSignature(string message, string signatureBase64, string publicKeyBase64)
        {
            var buf = Encoding.UTF8.GetBytes(message);

            var rsa = RSA.Create();
            
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKeyBase64), out _);

            var sign = Convert.FromBase64String(signatureBase64);

            var result = rsa.VerifyData(buf, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return result;
        }

        public static string ReadPublicKeyFromPem(string pemPublicKey)
        {
            return pemPublicKey
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "");
        }
    }
}