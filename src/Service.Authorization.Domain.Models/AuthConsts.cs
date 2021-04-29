using System;
using System.Text;
using MyJetWallet.Sdk.Service;

namespace Service.Authorization.Domain.Models
{
    public static class AuthConst
    {
        public const string SessionEncodingKeyEnv = "SESSION_ENCODING_KEY";

        public static byte[] GetSessionEncodingKey()
        {
            var key = GetSessionEncodingKeyOrigin();

            return Encoding.UTF8.GetBytes(key);
        }

        public static string GetSessionEncodingKeyToPrint()
        {
            var key = GetSessionEncodingKeyOrigin();

            return key.EncodeToSha1().ToHexString();
        }

        private static string _key = null;

        private static string GetSessionEncodingKeyOrigin()
        {
            if (!string.IsNullOrEmpty(_key))
                return _key;

            var key = Environment.GetEnvironmentVariable("SessionEncodingKeyEnv");

            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine($"Env Variable {SessionEncodingKeyEnv} is not found");
                throw new Exception($"Env Variable {SessionEncodingKeyEnv} is not found");
            }

            _key = key;

            return key;
        }


    }
}