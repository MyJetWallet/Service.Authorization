using System.Runtime.Serialization;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class AuthCredentialsGrpcModel
    {
        [DataMember(Order = 1)]
        public string EncodedEmail { get; set; }
        
        [DataMember(Order = 2)]
        public string Hash { get; set; }
        
        [DataMember(Order = 3)]
        public string Salt { get; set; }

        [DataMember(Order = 4)]
        public string Brand { get; set; }
        
        [DataMember(Order = 5)]
        public string Id { get; set; }
    }
}