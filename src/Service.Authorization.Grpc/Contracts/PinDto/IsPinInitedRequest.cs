using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts.PinDto;

[DataContract]
public class IsPinInitedRequest
{
    [DataMember(Order = 1)]
    public string ClientId { get; set; }
}