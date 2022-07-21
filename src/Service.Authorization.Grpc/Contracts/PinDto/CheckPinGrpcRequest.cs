using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts.PinDto;

[DataContract]
public class CheckPinGrpcRequest
{
    [DataMember(Order = 1)]
    public string ClientId { get; set; }
    
    [DataMember(Order = 2)]
    public string Hash { get; set; }
}