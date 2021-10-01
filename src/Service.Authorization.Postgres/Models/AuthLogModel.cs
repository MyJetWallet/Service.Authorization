using System;

namespace Service.Authorization.Postgres.Models
{
    public interface IAuthLogModel
    {
        string TraderId { get; }
        string Ip { get; }
        string UserAgent { get; }
        DateTime DateTime { get; }
        string Location { get; }
    }
    
    public class AuthLogModelDbModel : IAuthLogModel
    {
        public int Id { get; set; }
        public string TraderId { get; set;}
        public string Ip { get; set;}
        public string UserAgent { get; set;}
        public DateTime DateTime { get; set; }
        public string Location { get; set; }

        public static AuthLogModelDbModel Create(IAuthLogModel src)
        {
            return new AuthLogModelDbModel
            {
                TraderId = src.TraderId,
                Ip = src.Ip,
                UserAgent = src.UserAgent,
                DateTime = src.DateTime ,
                Location = src.Location
            };
        }
    }
}