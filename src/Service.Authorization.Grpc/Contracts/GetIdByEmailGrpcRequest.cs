using System.Collections.Generic;
using System.Runtime.Serialization;
using Destructurama.Attributed;

namespace Service.Authorization.Grpc.Contracts
{
    [DataContract]
    public class GetIdsByEmailGrpcRequest
    {
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [DataMember(Order = 1)]
        public string Email { get; set; }
        
        public static GetIdsByEmailGrpcRequest Create(string email)
        {
            return new GetIdsByEmailGrpcRequest
            {
                Email = email
            };
        }
    }

    [DataContract]
    public class GetIdsByEmailGrpcResponse
    {
        [DataMember(Order = 1)]
        public IEnumerable<TraderBrandGrpcModel> TraderBrands { get; set; }
    }
}