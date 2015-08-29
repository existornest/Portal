using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Profile;
using System.Web.Security;

namespace Portal.Library
{
    public class PasswordHash
    {
        private readonly string _salt;
        private readonly string _hash;
        private const int saltLength = 8;

        public string Salt { get { return _salt; } }
        public string Hash { get { return _hash; } }

        public static PasswordHash Create(string password)
        {
            string salt = CreateSalt();
            string hash = CalculateHash(salt, password);
            return new PasswordHash(salt, hash);
        }

        public static PasswordHash Create(string salt, string hash)
        {
            return new PasswordHash(salt, hash);
        }

        public static string GenerateRandomString(int length)
        {
            byte[] r = new byte[length];
            new RNGCryptoServiceProvider().GetBytes(r);
            return Convert.ToBase64String(r);
        }

        public bool Verify(string password)
        {
            string h = CalculateHash(_salt, password);
            return _hash.Equals(h);
        }

        private PasswordHash(string s, string h)
        {
            _salt = s;
            _hash = h;
        }

        private static string CreateSalt()
        {
            byte[] r = CreateRandomBytes(saltLength);
            return Convert.ToBase64String(r);
        }

        private static byte[] CreateRandomBytes(int len)
        {
            byte[] r = new byte[len];
            new RNGCryptoServiceProvider().GetBytes(r);
            return r;
        }

        private static string CalculateHash(string salt, string password)
        {
            byte[] data = _toByteArray(salt + password);
            byte[] hash = _calculateHash(data);
            return Convert.ToBase64String(hash);
        }

        private static byte[] _calculateHash(byte[] data)
        {
            return new SHA512CryptoServiceProvider().ComputeHash(data);
        }

        private static byte[] _toByteArray(string s)
        {
            return System.Text.Encoding.UTF8.GetBytes(s);
        }
    }
}