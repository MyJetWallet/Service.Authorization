using System;
using Destructurama.Attributed;
using Newtonsoft.Json;
using Service.Authorization.Domain;

namespace Service.Authorization.Postgres.Models
{
    public class AuthenticationCredentialsEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("salt")]
        public string Salt { get; set; }
        
        [JsonProperty("brand")]
        public string Brand { get; set; }

        public void SetPassword(string hash, string salt)
        {
            Salt = salt;
            Hash = hash;
        }
        
        public bool Authenticate(string hash, string salt)
        {
            return Hash == hash && Salt == salt;
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
        
        public static string DecodeEmail(string email, byte[] key, byte[] initVector)
        {
            return email.Decode(key,initVector).ToLower();
        }
        
        public static AuthenticationCredentialsEntity Create(string email, string hash, string salt, byte[] key, byte[] initVector, string brand)
        {
            var result = new AuthenticationCredentialsEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                Email = EncodeEmail(email, key, initVector),
                Brand = brand,
                Salt = salt,
                Hash = hash
            };
            
            return result;
        }
        
        public static AuthenticationCredentialsEntity Create(string id, string encodedEmail, string hash, string salt, string brand)
        {
            var result = new AuthenticationCredentialsEntity
            {
                Id = id,
                Email = encodedEmail,
                Brand = brand,
                Salt = salt,
                Hash = hash
            };
            return result;
        }
    }
}