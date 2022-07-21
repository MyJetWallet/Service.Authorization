using System;
using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts.PinDto;

[DataContract]
public class CheckPinGrpcResponse
{
    [DataMember(Order = 1)]
    public bool IsValid { get; set; }
    
    [DataMember(Order = 2)]
    public int Attempts { get; set; }
    
    [DataMember(Order = 3)]
    public TimeSpan BlockedTime { get; set; }
    
    [DataMember(Order = 4)]
    public bool TerminateSession { get; set; }
    
    [DataMember(Order = 5)]
    public bool BlockPin { get; set; }
}