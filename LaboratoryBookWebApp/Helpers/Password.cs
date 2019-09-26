using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Helpers
{
    public static class Password
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
        public static Tuple<bool, string> CheckPassword(string connectionString, int userId, string password)
        {            
            var connection = new MySqlConnection(connectionString);

            var commandString = $"SELECT salt FROM users WHERE user_id = '{userId}'; ";
            var sqlCommand = new MySqlCommand(commandString, connection);

            try
            {
                connection.Open();

                var salt = (string)sqlCommand.ExecuteScalar();

                if (string.IsNullOrEmpty(salt))
                {
                    return new Tuple<bool, string>(false, "Salt is empty");
                }

                var hash = GenerateHash(salt, password);

                sqlCommand.CommandText = $"SELECT count(*) FROM users" +
                                         $" WHERE user_id = '{userId}' AND password_hash ='{hash}'; ";

                var result = (long)sqlCommand.ExecuteScalar();

                if (result == 0)
                {
                    return new Tuple<bool, string>(false, "Old password does not match");
                }

                return new Tuple<bool, string>(true, "Password match!");
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

        }

        public static bool SetNewPassword(string connectionString, int userId, string newPassword)
        {
          
            var connection = new MySqlConnection(connectionString);

            var commandString = $"SELECT salt FROM users WHERE user_id = '{userId}'; ";
            var sqlCommand = new MySqlCommand(commandString, connection);

            try
            {
                connection.Open();

                var salt = (string)sqlCommand.ExecuteScalar();
                var hash = GenerateHash(salt, newPassword);

                commandString = $" UPDATE `users` SET `password_hash` = '{hash}' WHERE(`user_id` = '{userId}'); ";
                sqlCommand.CommandText = commandString;

                var updatePasswordResult = (long)sqlCommand.ExecuteNonQuery();

                if (updatePasswordResult == 0)
                    return false;
                else
                    return true;

            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }
        }
    }
}
