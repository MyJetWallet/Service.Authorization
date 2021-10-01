using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Service.Authorization.Postgres
{
    public static class EncoderDecoder
    {
        private static readonly char[] Digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        
        private static readonly Dictionary<char, byte> FirstByte = new Dictionary<char, byte>
        {
            ['0']=0,
            ['1']=1*16,
            ['2']=2*16,
            ['3']=3*16,
            ['4']=4*16,
            ['5']=5*16,
            ['6']=6*16,
            ['7']=7*16,
            ['8']=8*16,
            ['9']=9*16,
            ['A']=10*16,
            ['B']=11*16,
            ['C']=12*16,
            ['D']=13*16,
            ['E']=14*16,
            ['F']=15*16,
        };
        
        private static readonly Dictionary<char, byte> SecondByte = new Dictionary<char, byte>
        {
            ['0']=0,
            ['1']=1,
            ['2']=2,
            ['3']=3,
            ['4']=4,
            ['5']=5,
            ['6']=6,
            ['7']=7,
            ['8']=8,
            ['9']=9,
            ['A']=10,
            ['B']=11,
            ['C']=12,
            ['D']=13,
            ['E']=14,
            ['F']=15,
        };

        private static string ToHexString(this byte[] bytes)
        {
            var hex = new char[bytes.Length * 2];
            var index = 0;

            foreach (var b in bytes)
            {
                hex[index++] = Digits[b >> 4];
                hex[index++] = Digits[b & 0x0F];
            }

            return new string(hex);
        }
        
        private static byte[] HexStringToByteArray(this string hexString)
        {
            var arrayLen = hexString.Length /2;

            var i = 0;
            var j = 0;

            var result = new byte[arrayLen];
            
            while (i<hexString.Length)
            {
                var b1 = FirstByte[hexString[i]];
                i++;
                var b2 = SecondByte[hexString[i]];
                i++;

                result[j] = (byte)(b1 + b2);
                j++;
            }

            return result;
        }


        public static string Encode(this string src, byte[] initKey, byte[] initVector)
        {
            var bytes = Encoding.UTF8.GetBytes(src);
            // create a key from the password and salt, use 32K iterations – see note
            var key = new Rfc2898DeriveBytes(initKey, initVector, 32768);

            // create an AES object
            using Aes aes = new AesManaged();
            aes.KeySize = 256;
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            using var ms = new MemoryStream();

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytes, 0, bytes.Length);
                cs.Close();
            }

            return ms.ToArray().ToHexString();
        }

        public static string Decode(this string encryptedText, byte[] initKey, byte[] initVector)
        {
            var data = encryptedText.HexStringToByteArray();

            var key = new Rfc2898DeriveBytes(initKey, initVector, 32768);

            using Aes aes = new AesManaged();
            aes.KeySize = 256;
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);

            using MemoryStream ms = new MemoryStream();
            
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.Close();
            }
            
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}