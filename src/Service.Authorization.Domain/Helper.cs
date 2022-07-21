using System;
using System.Security.Cryptography;
using System.Text;

namespace Service.Authorization.Domain
{
    public static class AuthHelper
    {
        public static string GeneratePasswordHash(string password, string salt)
        {
            using var sha256Hash = SHA256.Create();
            
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var result = sha256Hash.ComputeHash(bytes);
            return Convert.ToBase64String(result);
        }
    }
}