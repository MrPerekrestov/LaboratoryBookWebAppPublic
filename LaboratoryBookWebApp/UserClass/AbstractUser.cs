using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LaboratoryBook.UserClass
{
    public abstract class User
    {
        public virtual int AccessID { get;  set; }
        public virtual int UserID { get;  set; }
        public virtual string UserName { get;  set; }

        public virtual int GetAccessID()
        {
            return AccessID;
        }

        public virtual int GetUserID() => UserID;
        public virtual string GetUserName() => UserName;
        

        public virtual bool SetUserName(string userName)
        {
            return true;
        }

        public virtual async Task<DataTable> GetDataFromLaboratoryBookAsync(string connectionString, string LaboratoryBookName, int Permission)
        {
            
            var connection = new MySqlConnection(connectionString);

            var commandString = $"SELECT * FROM `laboratory_book_{LaboratoryBookName}` WHERE `permissionID`<={(int)Permission};";
            var sqlCommand = new MySqlCommand(commandString, connection);

            await connection.OpenAsync();

            var result = new DataTable();
            var dbReader = await sqlCommand.ExecuteReaderAsync();
            result.Load(dbReader);

            await connection.CloseAsync();
            
            return result;
        }

        public virtual  DataTable GetDataFromLaboratoryBook(string connectionString, string LaboratoryBookName, int Permission)
        {            
            var connection = new MySqlConnection(connectionString);

            var commandString = $"SELECT * FROM `laboratory_book_{LaboratoryBookName}` WHERE `permissionID`<={(int)Permission};";
            var sqlCommand = new MySqlCommand(commandString, connection);

            var result = new DataTable();

            try
            {
                connection.Open();
                var dbReader = sqlCommand.ExecuteReader();
                result.Load(dbReader);
            }
            
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

            return result;
        }

        public virtual List<string> GetColumnNames(string LaboratoryBookName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["cs_login"].ConnectionString;
            var connection = new MySqlConnection(connectionString);

            var commandString = $"SHOW COLUMNS FROM `laboratory_book_{LaboratoryBookName}`;";
            var sqlCommand = new MySqlCommand(commandString, connection);

            
            var columnList = new List<string>();
            try
            {
                connection.Open();

                var result = new DataTable();
                var dbReader = sqlCommand.ExecuteReader();
                result.Load(dbReader);
                
                foreach(DataRow row in result.Rows)
                {
                    columnList.Add(row[0].ToString());
                }

            }
            
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

            return columnList;
        }

        public virtual async Task<DataTable> SearchInLaboratoryBookAsync(string LaboratoryBookName, int permission, string[] columnNames, string restriction)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["cs_login"].ConnectionString;
            var connection = new MySqlConnection(connectionString);

            var searchBuilder = new StringBuilder();

            searchBuilder.Append($"SELECT * FROM `laboratory_book_{LaboratoryBookName}`");
            searchBuilder.Append($" WHERE `permissionID`<={(int)permission} AND (");

            for (var i = 0; i < (columnNames.Length - 1); i++)
            {
                searchBuilder.Append($"`{columnNames[i]}` LIKE '%{restriction}%' OR");
            }

            searchBuilder.Append($"`{columnNames[columnNames.Length - 1]}` LIKE '%{restriction}%');");            

            var commandString = searchBuilder.ToString();
           
            var sqlCommand = new MySqlCommand(commandString, connection);

            var result = new DataTable();

            try
            {
                await connection.OpenAsync();

                var dbReader = await sqlCommand.ExecuteReaderAsync();
                result.Load(dbReader);
            }
            
            finally
            {
                await connection.CloseAsync();
                sqlCommand?.Dispose();
            }

            return result;
        }

        public virtual  DataTable SearchInLaboratoryBook(string LaboratoryBookName, int permission, string[] columnNames, string restriction)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["cs_login"].ConnectionString;
            var connection = new MySqlConnection(connectionString);

            var searchBuilder = new StringBuilder();

            searchBuilder.Append($"SELECT * FROM `laboratory_book_{LaboratoryBookName}`");
            searchBuilder.Append($" WHERE `permissionID`<={(int)permission} AND (");

            for (var i = 0; i < (columnNames.Length - 1); i++)
            {
                searchBuilder.Append($"`{columnNames[i]}` LIKE '%{restriction}%' OR");
            }

            searchBuilder.Append($"`{columnNames[columnNames.Length - 1]}` LIKE '%{restriction}%');");

            var commandString = searchBuilder.ToString();

            var sqlCommand = new MySqlCommand(commandString, connection);

            var result = new DataTable();

            try
            {
                connection.Open();
                var dbReader = sqlCommand.ExecuteReader();
                result.Load(dbReader);
            }
           
            finally
            {
                connection.Close();
                sqlCommand?.Dispose();
            }

            return result;
        }


    }
}
