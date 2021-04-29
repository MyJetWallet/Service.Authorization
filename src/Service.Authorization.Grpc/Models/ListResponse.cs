using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Models
{
    [DataContract]
    public class ListResponse<T>
    {
        [DataMember(Order = 1)] public List<T> List { get; set; }
    }
}