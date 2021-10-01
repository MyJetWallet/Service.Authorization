using System;
using Destructurama.Attributed;
using Newtonsoft.Json;

namespace Service.Authorization.Postgres.Models
{
    public class AuthenticationCredentialsEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("email")]
        public string Email { get; set; }

        [LogMasked(ShowFirst = 2, ShowLast = 2, PreserveLength = true)]
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [LogMasked(ShowFirst = 2, ShowLast = 2, PreserveLength = true)]
        [JsonProperty("salt")]
        public string Salt { get; set; }
        
        [JsonProperty("brand")]
        public string Brand { get; set; }

        public void SetPassword(string password)
        {
            Salt = Guid.NewGuid().ToString("N");
            Hash = AuthHelper.GeneratePasswordHash(password, Salt);
        }
        
        public bool Authenticate(string password)
        {
            var hash = AuthHelper.GeneratePasswordHash(password, Salt);
            return Hash == hash;
        }
        
        public static string EncodeEmail(string email, byte[] key, byte[] initVector)
        {
            return email.ToLower().Encode(key,initVector);
        }
        
        public static AuthenticationCredentialsEntity Create(string email, string password, byte[] key, byte[] initVector, string brand)
        {
            var result = new AuthenticationCredentialsEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                Email = EncodeEmail(email, key, initVector),
                Brand = brand
            };

            result.SetPassword(password);

            return result;
        }
    }
}