using LaboratoryBook.UserClass;
using LaboratoryBookWebApp.Enums;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Helpers
{
    public static class LaboratoryBookHelper
    {
        public static async Task<int> GetLaboratoryBookCreatorAsync(string connectionString, string laboratoryBook)
        {
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(laboratoryBook))
                throw new ArgumentNullException("connection string or laboratory book are empty");

            var connection = new MySqlConnection(connectionString);
            var commandString = $"SELECT creator_id FROM db_list WHERE db_name = '{laboratoryBook}';";
            var command = new MySqlCommand(commandString, connection);

            try
            {
                await connection.OpenAsync();
                var result = (int)(await command.ExecuteScalarAsync());
                return result;
            }
            finally
            {
                await connection.CloseAsync();
                command?.Dispose();
            }
        }
        public static int GetUserId(string connectionString, string userName)
        {
            var commandString = $"SELECT user_id FROM users WHERE user_name='{userName}'";
            return (int)GetDbDataScalar(connectionString, commandString);
        }
        public static int GetLaboratoryBookId(string connectionString, string laboratoryBookName)
        {
            var commandString = $"SELECT db_id FROM db_list WHERE db_name = '{laboratoryBookName}'";
            return (int)GetDbDataScalar(connectionString, commandString);
        }
        public static int DbNoQuery(string connectionString, string commandString)
        {
            var connection = new MySqlConnection(connectionString);
            var sqlCommand = new MySqlCommand(commandString, connection);

            try
            {
                connection.Open();

                var rowsAffected = (int)sqlCommand.ExecuteNonQuery();

                return rowsAffected;
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }
        }
        public static DataTable GetDbDataTable(string connectionString, string commandString)
        {
            var connection = new MySqlConnection(connectionString);
            var sqlCommand = new MySqlCommand(commandString, connection);

            try
            {
                connection.Open();

                var reader = sqlCommand.ExecuteReader();

                var dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }
        }
        public static object GetDbDataScalar(string connectionString, string commandString)
        {
            var connection = new MySqlConnection(connectionString);
            var sqlCommand = new MySqlCommand(commandString, connection);

            try
            {
                connection.Open();
                return sqlCommand.ExecuteScalar();
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }
        }
        public static bool ChangeDatum(string connectionString, string laboratoryBookName, string dataType, string dataValue, int Id)
        {
            int rowsAffected = 0;
            var connection = new MySqlConnection(connectionString);
            var sqlCommand = new MySqlCommand(String.Empty, connection);

            try
            {
                connection.Open();

                var commandString = string.Empty;

                dataValue = dataValue ?? string.Empty;

                if (string.IsNullOrEmpty(dataValue))
                {
                    commandString = $"UPDATE `laboratory_book_{laboratoryBookName}`" +
                                    $" SET `{dataType}` = NULL WHERE (`sampleID` = '{Id}')";
                }
                else
                {
                    commandString = $"UPDATE `laboratory_book_{laboratoryBookName}`" +
                                    $" SET `{dataType}` = '{dataValue}' WHERE (`sampleID` = '{Id}')";
                }

                sqlCommand.CommandText = commandString;

                rowsAffected = sqlCommand.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public static int GetPermissionId(string connectionString, string laboratoryBookName, int userId)
        {
            var connection = new MySqlConnection(connectionString);
            var commandString = "SELECT permission_id FROM db_list " +
                                "JOIN db_users " +
                                $"WHERE user_id = {userId} AND db_name = '{laboratoryBookName}'; ";

            var sqlCommand = new MySqlCommand(commandString, connection);
            int permission = 0;
            try
            {
                connection.Open();
                permission = (sbyte)(sqlCommand.ExecuteScalar());
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

            return permission;

        }
        public static User CurrentUser(string UserStatus)
        {
            switch (UserStatus)
            {
                case "Guest": return new Guest() { AccessID = 1 };
                case "Laborant": return new Laborant() { AccessID = 2 };
                case "Moderator": return new Moderator() { AccessID = 3 };
                case "Administer": return new Administer() { AccessID = 4 };
                default: return new Guest() { AccessID = 1 };
            }
        }
        public static string GetColumnList(string connectionString, string laboratoryBookName, ColumnToGet columnToGet)
        {
            var connection = new MySqlConnection(connectionString);

            string commandString = null;

            switch (columnToGet)
            {
                case ColumnToGet.Material:
                    commandString = $"SELECT `material` FROM materials_{laboratoryBookName};";
                    break;
                case ColumnToGet.Operator:
                    commandString = "SELECT `user_name` FROM `db_users` " +
                                    "JOIN `users` ON `users`.`user_id` = `db_users`.`user_id` " +
                                    "JOIN `db_list` ON `db_list`.`db_id` = `db_users`.`db_id` " +
                                    $"WHERE `db_name`= '{laboratoryBookName}';";
                    break;
                case ColumnToGet.PermissionID:
                    commandString = "SELECT `permission_id` FROM `permission`;";
                    break;
                case ColumnToGet.Regime:
                    commandString = $"SELECT `regime` FROM regimes_{laboratoryBookName};";
                    break;
                case ColumnToGet.Substrate:
                    commandString = $"SELECT `substrate` FROM substrates_{laboratoryBookName};";
                    break;
            }

            var sqlCommand = new MySqlCommand(commandString, connection);
            var requestedDataTable = new DataTable();

            try
            {
                connection.Open();

                var reader = sqlCommand.ExecuteReader();

                requestedDataTable.Load(reader);
            }
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

            var resultBuilder = new StringBuilder();
            foreach (DataRow dr in requestedDataTable.Rows)
            {
                resultBuilder.Append(dr[0].ToString() + ";");
            }
            var result = resultBuilder.ToString();
            if (string.IsNullOrEmpty(result)) return string.Empty;
            return result.Remove(result.Length - 1);
        }

    }
}
