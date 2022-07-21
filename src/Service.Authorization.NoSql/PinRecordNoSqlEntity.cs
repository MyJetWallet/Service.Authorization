using MyNoSqlServer.Abstractions;
using Service.Authorization.Domain.Models;

namespace Service.Authorization.NoSql;

public class PinRecordNoSqlEntity: MyNoSqlDbEntity
{
    public const string TableName = "jetwallet-auth-pin";
    public static string GeneratePartitionKey(string clientId) => clientId;
    public static string GenerateRowKey() => "pin";
    
    public PinRecord Pin { get; set; }

    public static PinRecordNoSqlEntity Create(PinRecord pin)
    {
        return new PinRecordNoSqlEntity()
        {
            PartitionKey = GeneratePartitionKey(pin.ClientId),
            RowKey = GenerateRowKey(),
            Pin = pin
        };
    }
}