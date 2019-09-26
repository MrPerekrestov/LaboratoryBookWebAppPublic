using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Helpers
{
    public class SelectHelper
    {
        public static IEnumerable<string> GetAvailableLaboratoryBooks(int userId, string connectionString)
        {
           
            var connection = new MySqlConnection(connectionString);
            var sqlCommand = new MySqlCommand
            {
                CommandText = "SELECT `db_name` FROM `db_list` "+
                              "JOIN `db_users` ON `db_list`.`db_id`=`db_users`.`db_id` "+
                              $"WHERE `user_id`= {userId}; ",
                Connection = connection
            };

            var laboratoryBooks = new DataTable();

            connection.Open();
            laboratoryBooks.Load(sqlCommand.ExecuteReader());
            connection.Close();           

            foreach (DataRow row in laboratoryBooks.Rows)
            {
                yield return row[0].ToString();
            }           

        }
    }
}
