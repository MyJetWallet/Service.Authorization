using MyNoSqlServer.Abstractions;
using Service.Authorization.Postgres;
using Service.Authorization.Postgres.Models;

namespace Service.Authorization.NoSql
{
    public class AuthenticationCredentialsCacheNoSqlEntity : MyNoSqlDbEntity
    {
        public static string GeneratePartitionKey(string email, byte[] key, byte[] initVector)
        {
            var encodedEmail = email.ToLower().Encode(key, initVector);
            return encodedEmail;
        }

        public static string EncodeEmail(string email, byte[] key, byte[] initVector)
        {
            var encodedEmail = email.ToLower().Encode(key, initVector);
            return encodedEmail;
        }

        public static string GeneratePartitionKey(string email)
        {
            return email;
        }

        public static string GeneratePartitionAsIdKey(string id)
        {
            return id.ToLower();
        }

        public static string GenerateRowKey(string brand)
        {
            return brand;
        }

        public string Id { get; set; }
        public string Email { get; set; }
        public string Brand { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }

        public bool Authenticate(string password)
        {
            var hash = AuthHelper.GeneratePasswordHash(password, Salt);
            return Hash == hash;
        }

        public static AuthenticationCredentialsCacheNoSqlEntity CreateByDatabaseAsset(
            AuthenticationCredentialsEntity databaseAsset)
        {
            return new AuthenticationCredentialsCacheNoSqlEntity
            {
                PartitionKey = GeneratePartitionKey(databaseAsset.Email),
                RowKey = GenerateRowKey(databaseAsset.Brand),
                Id = databaseAsset.Id,
                Salt = databaseAsset.Salt,
                Hash = databaseAsset.Hash,
                Email = databaseAsset.Email,
                Brand = databaseAsset.Brand
            };
        }
    }
}