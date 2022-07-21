using System.Runtime.Serialization;
using Service.Authorization.Domain.Models;

namespace Service.Authorization.Grpc.Contracts.PinDto;

[DataContract]
public class SetupPinGrpcRequest
{
    [DataMember(Order = 1)]
    public PinRecord Pin { get; set; }
    
    [DataMember(Order = 2)]
    public string RootSessionId { get; set; }
    
    [DataMember(Order = 3)]
    public string SessionId { get; set; }
    
    [DataMember(Order = 4)]
    public string Ip { get; set; }
    
    [DataMember(Order = 5)]
    public string IpCountry { get; set; }
    
    [DataMember(Order = 6)]
    public string UserAgent { get; set; }
}