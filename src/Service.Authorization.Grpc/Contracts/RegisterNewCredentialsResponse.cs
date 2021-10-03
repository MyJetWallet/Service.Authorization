using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class RegisterNewCredentialsResponse
    {
        [DataMember(Order = 1)]
        public string TraderId { get; set; }
    }
}