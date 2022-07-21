using System;
using System.Runtime.Serialization;

namespace Service.Authorization.Domain.Models
{
    [DataContract]
    public class PinRecord
    {
        [DataMember(Order = 1)]
        public string ClientId { get; set; }
    
        [DataMember(Order = 2)]
        public string Hash { get; set; }
        
        [DataMember(Order = 3)]
        public string Salt { get; set; }

        public bool CheckPin(string pin)
        {
            var value = AuthHelper.GeneratePasswordHash(pin, Salt);
            return value == Hash;
        }
        
        public void SetPin(string pin)
        {
            Salt = Guid.NewGuid().ToString("N");
            var value = AuthHelper.GeneratePasswordHash(pin, Salt);
            Hash = value;
        }
    }
}