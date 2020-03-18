using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using MyFace.Models.Database;
using MyFace.Models.Request;

namespace MyFace.Data
{
    public class HashSalt
    {
        public static string GetSalt()
        {
            var randomNum = new RNGCryptoServiceProvider();
            byte[] salt = new byte[32];
            randomNum.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        public static string HashPassword(string password, string salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.UTF8.GetBytes(salt),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 1,
            numBytesRequested: 64));
        }
    }
}