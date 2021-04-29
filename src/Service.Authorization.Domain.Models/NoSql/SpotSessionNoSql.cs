using System;
using Microsoft.VisualBasic;
using MyNoSqlServer.Abstractions;

namespace Service.Authorization.Domain.Models.NoSql
{
    public class SpotSessionNoSql: MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-session-active";

        public static string GeneratePartitionKey(string clientId) => clientId;
        public static string GenerateRowKey(string sessionRootId) => sessionRootId;

        public string RootId { get; set; }

        public string PublicKeyBase64 { get; set; }

        public string BrokerId { get; set; }
        public string BrandId { get; set; }
        public string ClientId { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime DiedDateTime { get; set; }

        public static SpotSessionNoSql Create(string brokerId, string brandId, string clientId, DateTime dueDateTime, string publicKeyBase64)
        {
            var rootId = Guid.NewGuid().ToString("N");

            return new SpotSessionNoSql()
            {
                PartitionKey = SpotSessionNoSql.GeneratePartitionKey(clientId),
                RowKey = SpotSessionNoSql.GenerateRowKey(rootId),
                BrokerId = brokerId,
                BrandId = brandId,
                ClientId = clientId,
                CreateDateTime = DateTime.UtcNow,
                DiedDateTime = dueDateTime,
                PublicKeyBase64 = publicKeyBase64,
                RootId = rootId,
                Expires = dueDateTime
            };
        }
    }
}