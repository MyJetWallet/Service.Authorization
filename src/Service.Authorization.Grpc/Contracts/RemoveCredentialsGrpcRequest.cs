using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts;

[DataContract]
public class RemoveCredentialsGrpcRequest
{
    [DataMember(Order = 1)]
    public string ClientId { get; set; }

    public static RemoveCredentialsGrpcRequest Create(string clientId)
    {
        return new RemoveCredentialsGrpcRequest
        {
            ClientId = clientId
        };
    }
}