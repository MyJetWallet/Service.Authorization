using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts.PinDto;

[DataContract]
public class CheckPinGrpcResponse
{
    [DataMember(Order = 1)]
    public bool IsValid { get; set; }
}