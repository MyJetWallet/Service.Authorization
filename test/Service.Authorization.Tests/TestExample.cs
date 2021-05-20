using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Service.Authorization.Domain.Models;
using SimpleTrading.Cryptography;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;

namespace Service.Authorization.Tests
{
    public class TestExample
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var token = new JetWalletToken()
            {
                Id = "token-1",
                Expires = DateTime.Parse("2021-01-01 12:44:45"),
                SessionRootId = "115",
                BrandId = "my-brand",
                BrokerId = "my-broker"
            };
            
            var key = Encoding.UTF8.GetBytes("1234567890");

            var session = token.IssueTokenAsBase64String(key);

            
            var (result, baseToken) = TokensManager.ParseBase64Token<AuthorizationToken>(session, key, token.Expires.AddSeconds(-1));

            Assert.AreEqual(TokenParseResult.Ok, result);

            Assert.AreEqual(token.Id, baseToken.Id);
            Assert.AreEqual(token.Expires, baseToken.Expires);

            var (result2, newToken) = TokensManager.ParseBase64Token<JetWalletToken>(session, key, token.Expires.AddSeconds(-1));
            Assert.AreEqual(TokenParseResult.Ok, result2);

            Assert.AreEqual(token.Id, newToken.Id);
            Assert.AreEqual(token.Expires, newToken.Expires);
            Assert.AreEqual(token.BrandId, newToken.BrandId);
            Assert.AreEqual(token.BrokerId, newToken.BrokerId);
            Assert.AreEqual(token.SessionRootId, newToken.SessionRootId);

            var (result3, newToken2) = TokensManager.ParseBase64Token<JetWalletToken>(session, key, token.Expires);
            Assert.AreEqual(TokenParseResult.Expired, result3);
        }

        [Test]
        public void Test2()
        {
            var token = new AuthorizationToken()
            {
                Id = "token-1",
                Expires = DateTime.Parse("2021-01-01 12:44:45")
            };

            var key = Encoding.UTF8.GetBytes("1234567890");

            var session = token.IssueTokenAsBase64String(key);


            var (result, baseToken) = TokensManager.ParseBase64Token<AuthorizationToken>(session, key, token.Expires.AddSeconds(-1));

            Assert.AreEqual(TokenParseResult.Ok, result);

            Assert.AreEqual(token.Id, baseToken.Id);
            Assert.AreEqual(token.Expires, baseToken.Expires);

            var (result2, newToken) = TokensManager.ParseBase64Token<JetWalletToken>(session, key, token.Expires.AddSeconds(-1));
            Assert.AreEqual(TokenParseResult.Ok, result2);

            Assert.AreEqual(token.Id, newToken.Id);
            Assert.AreEqual(token.Expires, newToken.Expires);
            Assert.AreEqual(null, newToken.BrandId);
            Assert.AreEqual(null, newToken.BrokerId);
            Assert.AreEqual(null, newToken.SessionRootId);

            var (result3, newToken2) = TokensManager.ParseBase64Token<JetWalletToken>(session, key, token.Expires);
            Assert.AreEqual(TokenParseResult.Expired, result3);
        }

        [Test]
        public void Signature()
        {
            // flutter: https://pub.dev/packages/fast_rsa

            var originalText = $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}";

            Console.WriteLine($"originalText: {originalText}");

            var buf = Encoding.UTF8.GetBytes(originalText);
            var bufBase64 = Convert.ToBase64String(buf);

            Console.WriteLine($"original base 64 string: {bufBase64}");

            var rsa = RSA.Create(2048);

            var privateKey = rsa.ExportRSAPrivateKey();
            var publicKey = rsa.ExportRSAPublicKey();

            Console.WriteLine($"PrivateKey base64: {Convert.ToBase64String(privateKey)}. Length: {privateKey.Length}");
            Console.WriteLine($"PublicKey base64: {Convert.ToBase64String(publicKey)}. Length: {publicKey.Length}");

            var sign = rsa.SignData(buf, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var signBase64 = Convert.ToBase64String(sign);
            Console.WriteLine($"sign base64: {signBase64}");

            Console.WriteLine();
            Console.WriteLine();

            var rsa2 = RSA.Create();
            rsa2.ImportRSAPublicKey(publicKey, out var read);

            Console.WriteLine($"read: {read}, length: {publicKey.Length}");

            var result = rsa2.VerifyData(buf, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Assert.IsTrue(result);
        }

        [Test]
        public void Signature2()
        {
            // flutter: https://pub.dev/packages/fast_rsa

            var originalText = "070098c1499e4760a5c1d7888f0e4378_2021-04-28T14:38:13";

            var buf = Encoding.UTF8.GetBytes(originalText);

            var rsa2 = RSA.Create();

            var signature = "Jb1fOu6tPxYPnf0vn7B4/lp17iaBTiLTExQ7O13TxqE8RywO6W1p8uQ1uAIfFIYcDFTGA06QKwk0lZyybMAjMIFtP59NsTyeicPVKnPfu+1ukTDt28yYRdUGj9ZMd1zHh5llaaH8tLHH8JG2WmyKAefuyk5vRanegftQfc5QcpDeKiobI3AVGhPUy7edOx1lMlWrIaKgR3mHc/psmDH2FVBZF5P3aOE6O6Pnn/4ZG0WzRoTTF9Btr+3qV5dry0NldlCQKSHldltEhQlBTWJwTBao24RDmgRuahR40WGn5d+5mersMsMVjP5nsI7Fp7HjEIJmvPd6nO7cYbBaTz5N2w==";
            rsa2.ImportRSAPublicKey(Convert.FromBase64String("MIIBCgKCAQEAzOmgHr6eI+2uDGCYkEg+aGGxcRwRWYL7g6ynwMxunYdPMw6KylymxP5bEGn9s7svfvQdklJNeqU/QdnyNflne70SHB4m7hNYimF8mNbJyUPGs4nIkHW2jtRmJUeWR3RYcB9upMsNWcZG2wej7oV5eDmVrF7haeMIrQKSU4/IypYgc5coZWf6EXAdjRPYddpjyS1GaatSBqVp66hlQB8GchcxogTxbWN/jcQp8VwAptK2hx5r/K9CH9DxWR0VM/m9OIbmrC5cKbksn41OtwpaMe/1KErODVbmVuYm/ol+TCO7CV2TumocF5VttjXLf59tV6ikrhMmuY8fUlnFW1ujvwIDAQAB"), out _);

            var sign = Convert.FromBase64String(signature);

            var result = rsa2.VerifyData(buf, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Assert.IsTrue(result);
        }

        [Test]
        public void Signature4()
        {
            // flutter: https://pub.dev/packages/fast_rsa

            var originalText = "070098c1499e4760a5c1d7888f0e4378_2021-04-28T14:38:13";
            var signature = "Jb1fOu6tPxYPnf0vn7B4/lp17iaBTiLTExQ7O13TxqE8RywO6W1p8uQ1uAIfFIYcDFTGA06QKwk0lZyybMAjMIFtP59NsTyeicPVKnPfu+1ukTDt28yYRdUGj9ZMd1zHh5llaaH8tLHH8JG2WmyKAefuyk5vRanegftQfc5QcpDeKiobI3AVGhPUy7edOx1lMlWrIaKgR3mHc/psmDH2FVBZF5P3aOE6O6Pnn/4ZG0WzRoTTF9Btr+3qV5dry0NldlCQKSHldltEhQlBTWJwTBao24RDmgRuahR40WGn5d+5mersMsMVjP5nsI7Fp7HjEIJmvPd6nO7cYbBaTz5N2w==";
            var publicKey = @"-----BEGIN PUBLIC KEY-----
MIIBCgKCAQEAzOmgHr6eI+2uDGCYkEg+aGGxcRwRWYL7g6ynwMxunYdPMw6Kylym
xP5bEGn9s7svfvQdklJNeqU/QdnyNflne70SHB4m7hNYimF8mNbJyUPGs4nIkHW2
jtRmJUeWR3RYcB9upMsNWcZG2wej7oV5eDmVrF7haeMIrQKSU4/IypYgc5coZWf6
EXAdjRPYddpjyS1GaatSBqVp66hlQB8GchcxogTxbWN/jcQp8VwAptK2hx5r/K9C
H9DxWR0VM/m9OIbmrC5cKbksn41OtwpaMe/1KErODVbmVuYm/ol+TCO7CV2Tumoc
F5VttjXLf59tV6ikrhMmuY8fUlnFW1ujvwIDAQAB
-----END PUBLIC KEY-----";

            var result = MyRsa.ValidateSignature(originalText, signature, MyRsa.ReadPublicKeyFromPem(publicKey));

            Assert.IsTrue(result);
        }

        [Test]
        public void Signature3()
        {
            // flutter: https://pub.dev/packages/fast_rsa

            var originalText = "070098c1499e4760a5c1d7888f0e4378_2021-04-28T14:38:13";
            var signature = "Jb1fOu6tPxYPnf0vn7B4/lp17iaBTiLTExQ7O13TxqE8RywO6W1p8uQ1uAIfFIYcDFTGA06QKwk0lZyybMAjMIFtP59NsTyeicPVKnPfu+1ukTDt28yYRdUGj9ZMd1zHh5llaaH8tLHH8JG2WmyKAefuyk5vRanegftQfc5QcpDeKiobI3AVGhPUy7edOx1lMlWrIaKgR3mHc/psmDH2FVBZF5P3aOE6O6Pnn/4ZG0WzRoTTF9Btr+3qV5dry0NldlCQKSHldltEhQlBTWJwTBao24RDmgRuahR40WGn5d+5mersMsMVjP5nsI7Fp7HjEIJmvPd6nO7cYbBaTz5N2w==";
            var publicKey = "MIIBCgKCAQEAzOmgHr6eI+2uDGCYkEg+aGGxcRwRWYL7g6ynwMxunYdPMw6KylymxP5bEGn9s7svfvQdklJNeqU/QdnyNflne70SHB4m7hNYimF8mNbJyUPGs4nIkHW2jtRmJUeWR3RYcB9upMsNWcZG2wej7oV5eDmVrF7haeMIrQKSU4/IypYgc5coZWf6EXAdjRPYddpjyS1GaatSBqVp66hlQB8GchcxogTxbWN/jcQp8VwAptK2hx5r/K9CH9DxWR0VM/m9OIbmrC5cKbksn41OtwpaMe/1KErODVbmVuYm/ol+TCO7CV2TumocF5VttjXLf59tV6ikrhMmuY8fUlnFW1ujvwIDAQAB";

            var result = MyRsa.ValidateSignature(originalText, signature, publicKey);

            Assert.IsTrue(result);
        }

        [Test]
        public void pubkey_get()
        {
            var key = @"-----BEGIN PUBLIC KEY-----
MIIBCgKCAQEAzOmgHr6eI+2uDGCYkEg+aGGxcRwRWYL7g6ynwMxunYdPMw6Kylym
xP5bEGn9s7svfvQdklJNeqU/QdnyNflne70SHB4m7hNYimF8mNbJyUPGs4nIkHW2
jtRmJUeWR3RYcB9upMsNWcZG2wej7oV5eDmVrF7haeMIrQKSU4/IypYgc5coZWf6
EXAdjRPYddpjyS1GaatSBqVp66hlQB8GchcxogTxbWN/jcQp8VwAptK2hx5r/K9C
H9DxWR0VM/m9OIbmrC5cKbksn41OtwpaMe/1KErODVbmVuYm/ol+TCO7CV2Tumoc
F5VttjXLf59tV6ikrhMmuY8fUlnFW1ujvwIDAQAB
-----END PUBLIC KEY-----";

            var data = key.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "");

            Assert.AreEqual("MIIBCgKCAQEAzOmgHr6eI+2uDGCYkEg+aGGxcRwRWYL7g6ynwMxunYdPMw6KylymxP5bEGn9s7svfvQdklJNeqU/QdnyNflne70SHB4m7hNYimF8mNbJyUPGs4nIkHW2jtRmJUeWR3RYcB9upMsNWcZG2wej7oV5eDmVrF7haeMIrQKSU4/IypYgc5coZWf6EXAdjRPYddpjyS1GaatSBqVp66hlQB8GchcxogTxbWN/jcQp8VwAptK2hx5r/K9CH9DxWR0VM/m9OIbmrC5cKbksn41OtwpaMe/1KErODVbmVuYm/ol+TCO7CV2TumocF5VttjXLf59tV6ikrhMmuY8fUlnFW1ujvwIDAQAB",
                data);

        }

        [Test]
        public void pubkey_get2()
        {
            var key = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzpC++GLOCNtmFtXi81Fc
oOkLx29BnVCDhJU3vVLvUbIOAVMkPjFskdA/vucsXDvydV/LktN7M5fmRmknBqm9
HzxDOMxytY7Wo9Qc+GK+1wEGmgYuhxpHA9GEZDTp3924RQKQywtr1n80K06Sle9c
1HW6sZ9EhnXOrqM+o4J384M4xjRACW/mfIXmoSK2JWxmYD/TP7zK9xJx1J7ZNsC5
GMmkvxdVoQHB69LOVT3X+avYzbxtE8bJAyTRqB+k4fLZStYTCTvNZ1LpOdvxIHCl
xRTXrtxsuM1w6GPMbMqljQYXSAwJZb2y9ggDw8P53qukKENBy6mKyGnFYliS2T00
+QIDAQAB
-----END PUBLIC KEY-----
";

            var data = key.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "");

            Assert.AreEqual("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzpC++GLOCNtmFtXi81FcoOkLx29BnVCDhJU3vVLvUbIOAVMkPjFskdA/vucsXDvydV/LktN7M5fmRmknBqm9HzxDOMxytY7Wo9Qc+GK+1wEGmgYuhxpHA9GEZDTp3924RQKQywtr1n80K06Sle9c1HW6sZ9EhnXOrqM+o4J384M4xjRACW/mfIXmoSK2JWxmYD/TP7zK9xJx1J7ZNsC5GMmkvxdVoQHB69LOVT3X+avYzbxtE8bJAyTRqB+k4fLZStYTCTvNZ1LpOdvxIHClxRTXrtxsuM1w6GPMbMqljQYXSAwJZb2y9ggDw8P53qukKENBy6mKyGnFYliS2T00+QIDAQAB",
                data);

        }

        [Test]
        public void TTtt()
        {
            var session = "phh8Sc2vLgde4Ds9aeQY5eaXzjN7FImLCFvE3TDwC8wXlcvwKQ5Ns0soZ/BPxwZzNUBnbyac/ElkpFg/sxKhYyMzceEr/aYNvW1TkAgQmu1quaXumfgREix0RCEo4iqjAEgJnoaivND8WpoBp0HyOkSLM6Z5xsxSOu0ID6EikVXn8xNrJY7run4w9/ZYa88Z";

            var (result, baseToken) = TokensManager.ParseBase64Token<JetWalletToken>(session, Encoding.UTF8.GetBytes("e537d941-f7d2-4939-b97b-ae4722ca56aa"), DateTime.UtcNow);

            Console.WriteLine(result);
        }


        [Test]
        public void TTtt2()
        {
            var session = "u4uVXItnwcLG0rdTULo7LotsqiQNw1DbpvIkLCEMnG8xb9lREXY5h4GRlOiIiaFipKTZw7oS0KkZuryJrj0L4x1uG3hMDtBLY2yRQ/hsEX478wIr5gePCEltZ5H49WyLivLu2uxL1DmTAbDPQjKknPM+xlXz4kXSDZ7ZJz88LOMNUE8tMRp73R/2uINRUBZK5w+Xbw8Yz9Bii7RQLvM+RZBLLzPr9I0mi4o6UvDOXmmtLyoJeoQU5+iZgJ9tdzHJtEAaLCyWlBx1At0u6E+llA==";

            var (result, baseToken) = TokensManager.ParseBase64Token<JetWalletToken>(session, Encoding.UTF8.GetBytes("e537d941-f7d2-4939-b97b-ae4722ca56aa"), DateTime.UtcNow);

            Console.WriteLine(result);

            Console.WriteLine(JsonConvert.SerializeObject(baseToken));
        }


    }
}
