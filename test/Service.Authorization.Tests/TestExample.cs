using System;
using NUnit.Framework;
using Service.Authorization.Domain;

namespace Service.Authorization.Tests
{
    public class TestExample
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_hash()
        {
            var pass = "1111";
            var salt = "qqqqqqqqq";
            var hash = AuthHelper.GeneratePasswordHash(pass, salt);
            Console.WriteLine($"hash: {hash}");
            Assert.Pass();
        }
    }
}
