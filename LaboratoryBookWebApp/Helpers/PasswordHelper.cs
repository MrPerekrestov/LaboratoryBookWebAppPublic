using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Helpers
{
    public class PasswordHelper
    {
        public static string GenerateSalt()
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var builder = new StringBuilder();
            for (var i = 0; i < salt.Length; i++)
            {
                builder.Append(salt[i].ToString("x2"));
            }

            return builder.ToString();
        }

        public static string GenerateHash(string salt, string password)
        {
            using (var SHA256hash = SHA256.Create())
            {
                string stringToHash = salt + password;
                var bytes = SHA256hash.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));

                var builder = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
