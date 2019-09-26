using LaboratoryBookWebApp.Enums;
using LaboratoryBookWebApp.Models;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Helpers
{
    public static class LoginHelper
    {
        public static Tuple<bool, string, LoginUserModel> ValidateLoginAndPassword(string userName, string password, string connectionString)
        {
            var connection = new MySqlConnection(connectionString);

            var commandString = $"SELECT count(*) FROM users WHERE user_name = '{userName}';";
            var sqlCommand = new MySqlCommand(commandString, connection);

            try
            {
                connection.Open();
                var userCheckResult = (long)(sqlCommand.ExecuteScalar());

                if (userCheckResult == 0)
                {
                    return new Tuple<bool, string, LoginUserModel>(false, $"User '{userName}' does not exist", null);
                }
                else
                {
                    sqlCommand.CommandText = $"SELECT `salt` FROM `users` WHERE `user_name` = '{userName}'";
                    var salt = (string)sqlCommand.ExecuteScalar();

                    var generatedHash = PasswordHelper.GenerateHash(salt, password);

                    sqlCommand.CommandText = $"SELECT count(*) FROM users WHERE user_name = '{userName}' AND password_hash ='{generatedHash}';";
                    var passwordAndUserCheckResult = (long)sqlCommand.ExecuteScalar();

                    if (passwordAndUserCheckResult > 0)
                    {
                        var user = GetUserByName(userName, connectionString);

                        return new Tuple<bool, string, LoginUserModel>(true, "Validation success", user);
                    }
                    else
                    {
                        return new Tuple<bool, string, LoginUserModel>(false, "Wrong password", null);
                    }

                }
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

        }

        public static LoginUserModel GetUserByName(string userName, string connectionString)
        {
            var connection = new MySqlConnection(connectionString);
            var sqlCommand = new MySqlCommand
            {
                CommandText = $"SELECT `status_id` FROM `users` WHERE `user_name` = '{userName}';",
                Connection = connection
            };

            try
            {
                connection.Open();
                var statusSbyte = (sbyte)(sqlCommand.ExecuteScalar());

                var userStatus = string.Empty;
                switch(statusSbyte)
                {
                    case 1: userStatus = "Guest";
                            break;
                    case 2: userStatus = "Laborant";
                            break;
                    case 3: userStatus = "Moderator";
                            break;
                    case 4: userStatus = "Administer";
                            break;
                    default: userStatus = "Guest";
                             break;

                }

                sqlCommand.CommandText = $"SELECT `user_id` FROM users WHERE user_name = '{userName}'; ";
                var userID = (int)(sqlCommand.ExecuteScalar());

                var result = new LoginUserModel()
                {
                    UserId = userID,
                    UserName = userName,
                    UserStatus = userStatus
                };

                return result;

            }

            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

        }
    }
}
