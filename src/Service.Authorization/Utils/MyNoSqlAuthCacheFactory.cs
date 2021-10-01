using System;
using MyNoSqlServer.DataReader;
using MyNoSqlServer.DataWriter;
using Service.Authorization.Utils;

namespace Service.Authorization.NoSql
{
    public static class MyNoSqlAuthCacheFactory
    {
        private const string AuthCache = "auth-cache";

        public static AuthenticationCredentialsCacheReader CreateAuthCacheMyNoSqlReader(
            this MyNoSqlTcpClient client, byte[] key, byte[] initVector)
        {
            return new AuthenticationCredentialsCacheReader(
                new MyNoSqlReadRepository<AuthenticationCredentialsCacheNoSqlEntity>(client,
                    AuthCache), key, initVector);
        }

        public static AuthenticationCredentialsCacheWriter CreateAuthCacheNoSqlWriter(Func<string> getUrl, byte[] key,
            byte[] initVector)
        {
            return new AuthenticationCredentialsCacheWriter(
                new MyNoSqlServerDataWriter<AuthenticationCredentialsCacheNoSqlEntity>(getUrl,
                    AuthCache, true), key, initVector);
        }
    }
}