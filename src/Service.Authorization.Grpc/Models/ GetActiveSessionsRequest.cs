using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Models
{
    [DataContract]
    public class GetActiveSessionsRequest
    {
        [DataMember(Order = 1)] public string ClientId { get; set; }
    }
}