using LaboratoryBookWebApp.Helpers;
using LaboratoryBookWebApp.Models;
using Microsoft.Extensions.FileProviders;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace LaboratoryBook.UserClass
{
    public class Administer : User, IAdvancedUser
    {

        public Administer(string userName, int userID)
        {
            this.UserName = userName;
            this.AccessID = 4;
            this.UserID = userID;
        }
        public Administer()
        {

        }
        private string GetDeleteBookCommandString(string deleteResourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourseStream = assembly.GetManifestResourceStream(deleteResourcePath);
            var resourseList = assembly.GetManifestResourceNames();
            var deleteLaboratoryBookCommand = string.Empty;

            using (var streamReader = new StreamReader(resourseStream))
            {
                deleteLaboratoryBookCommand = streamReader.ReadToEnd();
            }

            return deleteLaboratoryBookCommand;
        }
        private string SetProperLaboratoryBookNameAndId(
            string deleteLaboratoryBookCommand,
            string laboratoryBookName,
            int laboratoryBookId)
        {
            return deleteLaboratoryBookCommand
                    .Replace("[NewLaboratoryBookId]", laboratoryBookId.ToString())
                    .Replace("test_db", laboratoryBookName)
                    .Replace("\r\n", string.Empty);
        }

        public async Task<bool> DeleteLaboratryBookAsync(DeleteLaboratoryBookOptions options)
        {
            var deleteCommandString = GetDeleteBookCommandString(options.EmbeddedResourcePath);

            deleteCommandString = SetProperLaboratoryBookNameAndId(
                deleteCommandString,
                options.LaboratoryBookName,
                options.LaboratoryBookId
                );
            var connection = new MySqlConnection(options.ConnectionString);
            var command = new MySqlCommand(deleteCommandString, connection);
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                return true;
            }
            finally
            {
                await connection.CloseAsync();
                command?.Dispose();
            }
        }
        public async Task<bool> CreateLaboratoryBook(CreateLaboratoryBookOptions options)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var createLaboratoryBookStream = assembly
                .GetManifestResourceStream("LaboratoryBookWebApp.TextFiles.CreateLaboratoryBookTemplate.txt");
            string createLaboratoryBookSQLCommands;
            //get  sql commands from the embedded resource
            using (var resourceReader = new StreamReader(createLaboratoryBookStream))
            {
                createLaboratoryBookSQLCommands = await resourceReader.ReadToEndAsync();
            }

            var connection = new MySqlConnection(options.ConnectionString);

            createLaboratoryBookSQLCommands = createLaboratoryBookSQLCommands
                .Replace("test_db", options.LaboratoryBookName);
            var sqlCommand = new MySqlCommand(String.Empty, connection);
            try
            {
                await connection.OpenAsync();
                //check if book exists
                sqlCommand.CommandText = $"SELECT count(*) FROM `db_list` WHERE db_name = '{options.LaboratoryBookName}';";
                var checkName = await sqlCommand.ExecuteScalarAsync();
                if ((long)checkName > 0)// return false;
                {
                    return false;
                }
                //get user_id
                sqlCommand.CommandText = $"SELECT `user_id` FROM `users` WHERE user_name = '{options.UserName}';";
                var userID = await sqlCommand.ExecuteScalarAsync();
                //add database to db_list
                sqlCommand.CommandText = $"INSERT INTO `db_list` (`db_name`, `creator_id`) VALUES ('{options.LaboratoryBookName}', '{options.UserId}');";
                var result = await sqlCommand.ExecuteNonQueryAsync();

                //create all tables
                sqlCommand.CommandText = createLaboratoryBookSQLCommands;
                result = await sqlCommand.ExecuteNonQueryAsync();

                //get db_id
                sqlCommand.CommandText = $"SELECT `db_id` FROM `db_list` WHERE `db_name` = '{options.LaboratoryBookName}';";
                var bookID = await sqlCommand.ExecuteScalarAsync();

                //link creator of database with database
                sqlCommand.CommandText = $"INSERT INTO `db_users` (`user_id`, `db_id`, `permission_id`) VALUES ('{options.UserId}', '{bookID}', '4');";
                result = await sqlCommand.ExecuteNonQueryAsync();

                //add statistics 
                sqlCommand.CommandText = $"INSERT INTO `statistics` (`db_name`, `user_name`, `time_changed`) " +
                                         $"VALUES ('{options.LaboratoryBookName}', '{options.UserName}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}');";
                result = await sqlCommand.ExecuteNonQueryAsync();
            }
            finally
            {
                await connection.CloseAsync();
                sqlCommand?.Dispose();
            }
            return true;
        }
        public bool ChangeUserStatus(string connectionString, ModifyUserModel changedUser)
        {
            var connection = new MySqlConnection(connectionString);
            var commandString = $"UPDATE `users` SET `status_id` = '{changedUser.UserStatusId}'" +
                                $" WHERE (`user_id` = '{changedUser.UserId}');";

            var sqlCommand = new MySqlCommand(commandString, connection);
            try
            {
                connection.Open();
                var result = (int)sqlCommand.ExecuteNonQuery();
                if (result > 0) return true;
                return false;
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }
        }

        public int CreateUser(string connectionString, string userName, string password, int accessId)
        {

            var salt = PasswordHelper.GenerateSalt();
            var hash = PasswordHelper.GenerateHash(salt, password);

            var connection = new MySqlConnection(connectionString);

            var commandString = "INSERT INTO `users` (`user_name`, `salt`, `password_hash`, `status_id`, `creator_id`) " +
                               $"VALUES ('{userName}', '{salt}', '{hash}', '{accessId}', '{this.GetUserID()}');";
            var sqlCommand = new MySqlCommand(commandString, connection);

            try
            {
                connection.Open();
                var result = sqlCommand.ExecuteNonQuery();
                return result;
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }
        }

        public List<AccessStatusModel> GetAccessStatusList(string connectionString)
        {
            var connection = new MySqlConnection(connectionString);

            var commandString = "SELECT * FROM access_status ORDER BY access_status_id; ";
            var sqlCommand = new MySqlCommand(commandString, connection);
            try
            {
                var result = new List<AccessStatusModel>();
                var usersTable = new DataTable();

                connection.Open();

                var dbReader = sqlCommand.ExecuteReader();
                usersTable.Load(dbReader);

                foreach (DataRow row in usersTable.Rows)
                {
                    
                    var accessStatusId = (int)row[0];
                    if (accessStatusId >= this.AccessID) continue;
                    var accessStatusName = (string)row[1];

                    var accessStatusModel = new AccessStatusModel(accessStatusId, accessStatusName);
                    result.Add(accessStatusModel);
                }

                return result;
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }
        }

        public List<ModifyUserModel> GetAvailableUsers(string connectionString)
        {

            var connection = new MySqlConnection(connectionString);

            var commandString = "SELECT user_id, user_name, status_id FROM users " +
                                $"WHERE status_id <= '{AccessID}'; ";
            var sqlCommand = new MySqlCommand(commandString, connection);
            try
            {

                var result = new List<ModifyUserModel>();
                var usersTable = new DataTable();

                connection.Open();

                var dbReader = sqlCommand.ExecuteReader();
                usersTable.Load(dbReader);

                foreach (DataRow row in usersTable.Rows)
                {
                    var userId = (int)row[0];
                    var userName = (string)row[1];
                    var statusId = (int)((sbyte)row[2]);

                    var modifyUserModel = new ModifyUserModel(userId, userName, statusId);
                    result.Add(modifyUserModel);
                }

                return result;
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }
        }

        public bool RemoveUser(string connectionString, int userId)
        {
            var connection = new MySqlConnection(connectionString);

            var commandString = $"DELETE FROM `users` WHERE (`user_id` = '{userId}');";

            var sqlCommand = new MySqlCommand(commandString, connection);

            try
            {

                connection.Open();

                var result = sqlCommand.ExecuteNonQuery();

                return result > 0;
            }

            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

        }

    }
}
