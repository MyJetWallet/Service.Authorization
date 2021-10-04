using System;
using MyNoSqlServer.Abstractions;
using Service.Authorization.Postgres;

namespace Service.Authorization.NoSql
{
    public class AuthenticationCredentialsCacheReader
    {
        private readonly IMyNoSqlServerDataReader<AuthenticationCredentialsCacheNoSqlEntity> _readRepository;
        private readonly byte[] _key;
        private readonly byte[] _initVector;

        public AuthenticationCredentialsCacheReader(
            IMyNoSqlServerDataReader<AuthenticationCredentialsCacheNoSqlEntity> repository,
            byte[] key, byte[] initVector)
        {
            _readRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            _key = key;
            _initVector = initVector;
        }

        public AuthenticationCredentialsCacheNoSqlEntity GetById(string id, string brand)
        {
            var partitionKey = AuthenticationCredentialsCacheNoSqlEntity.GeneratePartitionAsIdKey(id);

            var rowKey = AuthenticationCredentialsCacheNoSqlEntity.GenerateRowKey(brand);
            var result = _readRepository.Get(partitionKey, rowKey);

            if (result != null)
                result.Email = result.Email.Decode(_key, _initVector);

            return result;
        }

        public AuthenticationCredentialsCacheNoSqlEntity GetByEmail(string email, string brand)
        {
            var partitionKey = AuthenticationCredentialsCacheNoSqlEntity.GeneratePartitionKey(email, _key, _initVector);

            var rowKey = AuthenticationCredentialsCacheNoSqlEntity.GenerateRowKey(brand);

            return _readRepository.Get(partitionKey, rowKey);
        }
    }
}