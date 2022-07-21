using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts.PinDto;

[DataContract]
public class IsPinInitedResponse
{
    [DataMember(Order = 1)]
    public bool IsInited { get; set; }
}