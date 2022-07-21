using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts.PinDto;

[DataContract]
public class ResetPinGrpcResponse
{
    [DataMember(Order = 1)]
    public bool IsSuccess { get; set; }
    
    [DataMember(Order = 2)]
    public bool IsAlreadyExist { get; set; }
}