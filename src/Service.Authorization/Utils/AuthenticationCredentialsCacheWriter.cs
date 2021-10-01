using System;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using Service.Authorization.NoSql;
using Service.Authorization.Postgres.Models;

namespace Service.Authorization.Utils
{
    public class AuthenticationCredentialsCacheWriter
    {
        private readonly IMyNoSqlServerDataWriter<AuthenticationCredentialsCacheNoSqlEntity> _table;
        private readonly byte[] _key;
        private readonly byte[] _initVector;

        public AuthenticationCredentialsCacheWriter(
            IMyNoSqlServerDataWriter<AuthenticationCredentialsCacheNoSqlEntity> table, 
            byte[] key, 
            byte[] initVector
            )
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _key = key;
            _initVector = initVector;
        }

        public async Task<AuthenticationCredentialsCacheNoSqlEntity> AddEncodedByDatabaseEntity(AuthenticationCredentialsEntity entity)
        {
            entity.Email = AuthenticationCredentialsCacheNoSqlEntity.EncodeEmail(entity.Email, _key, _initVector);
            return await AddByDatabaseEntity(entity);
        }

        public async Task<AuthenticationCredentialsCacheNoSqlEntity> AddByDatabaseEntity(AuthenticationCredentialsEntity entity)
        {
            var cacheEntity = AuthenticationCredentialsCacheNoSqlEntity.CreateByDatabaseAsset(entity);

            await _table.InsertOrReplaceAsync(cacheEntity);

            return cacheEntity;
        }

        public async Task PurgeCache(int maxAmount)
        {
            await _table.CleanAndKeepMaxPartitions(maxAmount);
        }

        public async Task DeleteAsync(string email, string brand)
        {
            var pr = AuthenticationCredentialsCacheNoSqlEntity.GeneratePartitionKey(email, _key, _initVector);
            var rk = AuthenticationCredentialsCacheNoSqlEntity.GenerateRowKey(brand);
            
            await _table.DeleteAsync(pr, rk);
        }
    }
}