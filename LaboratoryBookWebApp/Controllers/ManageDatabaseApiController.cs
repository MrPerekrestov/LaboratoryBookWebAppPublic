using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaboratoryBookWebApp.Attributes;
using LaboratoryBookWebApp.Helpers;
using LaboratoryBookWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LaboratoryBookWebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    [AjaxOnlyController]
    public class ManageDatabaseApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ManageDatabaseApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpDelete("{list}/{listValue}")]        
        public Tuple<bool, object> RemoveListValueFromDatabase(
            string list,
            string listValue)
        {
            try
            {
                var userPermission = HttpContext
                   .User
                   .Claims
                   .First(claim => claim.Type == "Permission")
                   .Value;

                var userPermissionInt = int.Parse(userPermission);
                if (userPermissionInt < 2)
                {
                    return new Tuple<bool, object>(false, "You do not have a permission to remove list value");
                }

                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");
                var laboratoryBookName = HttpContext.User.Claims.First(claim => claim.Type == "LaboratoryBook").Value;

                var commandString = $"DELETE FROM `{list}s_{laboratoryBookName}` " +
                                    $"WHERE (`{list}` = '{listValue}'); ";

                var removeListValueResult = LaboratoryBookHelper.DbNoQuery(
                    connectionString,
                    commandString);

                if (removeListValueResult > 0)
                {
                    return new Tuple<bool, object>(true, "List value was successfully removed");
                }
                else
                {
                    return new Tuple<bool, object>(false, "Lista value was not removed");
                }

            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }
        }

        [HttpPut]      
        public Tuple<bool, object> AddListValueToDatabase([FromBody]AddListValueModel listValueModel)
        {
            try
            {
                var userPermission = HttpContext
                  .User
                  .Claims
                  .First(claim => claim.Type == "Permission")
                  .Value;

                var userPermissionInt = int.Parse(userPermission);
                if (userPermissionInt < 2)
                {
                    return new Tuple<bool, object>(false, "You do not have a permission to add list value");
                }
                var listName = listValueModel.listName;
                var listValue = listValueModel.listValue;

                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");
                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                var commandString = $"INSERT INTO `{listName}s_{laboratoryBookName}` (`{listName}`) " +
                                   $"VALUES ('{listValue}');";

                var addListValueResult = LaboratoryBookHelper.DbNoQuery(
                    connectionString,
                    commandString);

                if (addListValueResult > 0)
                {
                    return new Tuple<bool, object>(true, "List value was successfully added");
                }
                else
                {
                    return new Tuple<bool, object>(false, "List value was not added");
                }

            }
            catch (Exception excepton)
            {
                return new Tuple<bool, object>(false, excepton.Message);
            }
        }

        [HttpDelete("{columnName}")]       
        public Tuple<bool, object> RemoveColumnFromDatabase(string columnName)
        {
            try
            {
                var userPermission = HttpContext
                  .User
                  .Claims
                  .First(claim => claim.Type == "Permission")
                  .Value;

                var userPermissionInt = int.Parse(userPermission);
                if (userPermissionInt < 3)
                {
                    return new Tuple<bool, object>(false, "You do not have a permission to remove columns");
                }
                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                var commandString = $"ALTER TABLE `laboratory_book_{laboratoryBookName}` " +
                                $"DROP COLUMN `{columnName}`; ";

                var removeColumnResult = LaboratoryBookHelper.DbNoQuery(
                    connectionString,
                    commandString);

                return new Tuple<bool, object>(true, "Column was successfully removed");
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }
        }

        [HttpPut]      
        public Tuple<bool, object> AddColumnToDatabase([FromBody]AddColumnModel addColumnModel)
        {
            try
            {
                var userPermission = HttpContext
                  .User
                  .Claims
                  .First(claim => claim.Type == "Permission")
                  .Value;

                var userPermissionInt = int.Parse(userPermission);
                if (userPermissionInt < 3)
                {
                    return new Tuple<bool, object>(false, "You do not have a permission to remove list value");
                }
                var columnName = addColumnModel.columnName;
                var columnType = addColumnModel.columnType;
                var afterColumn = addColumnModel.afterColumn;

                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");
                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                var commandString = $"ALTER TABLE `laboratory_book_{laboratoryBookName}` " +
                                    $"ADD COLUMN `{columnName}` {columnType} NULL AFTER `{afterColumn}`; ";

                var addColumnResult = LaboratoryBookHelper.DbNoQuery(
                    connectionString,
                    commandString);

                return new Tuple<bool, object>(true, "Column was successfully added");
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }
        }

        [HttpDelete("{userId}")]        
        public Tuple<bool, object> RemoveUserFromDatabase(int userId)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                var laboratoryBookId = LaboratoryBookHelper.GetLaboratoryBookId(
                        connectionString,
                        laboratoryBookName
                    );
                var commandString = "DELETE FROM `db_users` " +
                                   $"WHERE(`user_id` = '{userId}') and (`db_id` = '{laboratoryBookId}');";
                var removeUserResult = LaboratoryBookHelper.DbNoQuery(
                        connectionString,
                        commandString
                    );
                if (removeUserResult > 0)
                {
                    return new Tuple<bool, object>(true, "User was successfully removed");
                }
                else
                {
                    return new Tuple<bool, object>(false, "User was not removed");
                }
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }
        }

        [HttpPut]        
        public Tuple<bool, object> AddUserToDatabase([FromBody]AddUserToDatabaseModel addUserToDatabaseModel)
        {
            try
            {
                var userPermission = HttpContext
                  .User
                  .Claims
                  .First(claim => claim.Type == "Permission")
                  .Value;

                var userPermissionInt = int.Parse(userPermission);
                if (userPermissionInt < 3)
                {
                    return new Tuple<bool, object>(false, "You do not have a permission to add user.");
                }
                var permissionId = addUserToDatabaseModel.permissionId;
                var userId = addUserToDatabaseModel.userId;

                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                var laboratoryBookId = LaboratoryBookHelper.GetLaboratoryBookId(
                        connectionString,
                        laboratoryBookName
                    );
                var commandString = "INSERT INTO `db_users` (`user_id`, `db_id`, `permission_id`)" +
                                    $" VALUES ('{userId}', '{laboratoryBookId}', '{permissionId}');";

                var addUserResult = LaboratoryBookHelper.DbNoQuery(
                    connectionString,
                    commandString);

                if (addUserResult > 0)
                {
                    return new Tuple<bool, object>(true, "User was successfully added");
                }
                else
                {
                    return new Tuple<bool, object>(false, "User was not added");
                }
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }
        }

        [HttpPut]        
        public Tuple<bool, object> UpdateListValue([FromBody] UpdateListValueModel updateListValueModel)
        {
            try
            {
                var userPermission = HttpContext
                  .User
                  .Claims
                  .First(claim => claim.Type == "Permission")
                  .Value;

                var userPermissionInt = int.Parse(userPermission);
                if (userPermissionInt < 2)
                {
                    return new Tuple<bool, object>(false, "You do not have a permission to update list value");
                }
                var listName = updateListValueModel.listName;
                var oldListValue = updateListValueModel.oldListValue;
                var newListValue = updateListValueModel.newListValue;

                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                var commandString = $"UPDATE `{listName}s_{laboratoryBookName}` " +
                                $"SET `{listName}` = '{newListValue}' " +
                                $"WHERE (`{listName}` = '{oldListValue}'); ";

                var listValueUpdateResult = LaboratoryBookHelper.DbNoQuery(
                    connectionString,
                    commandString
                    );

                if (listValueUpdateResult > 0)
                {
                    return new Tuple<bool, object>(true, "List value was successfully updated");
                }
                else
                {
                    return new Tuple<bool, object>(false, "List value was not updated");
                }

            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }
        }

        [HttpPut]        
        public Tuple<bool, object> UpdateColumn([FromBody] UpdateColumnModel updateColumnModel)
        {
            try
            {
                var userPermission = HttpContext
                  .User
                  .Claims
                  .First(claim => claim.Type == "Permission")
                  .Value;

                var userPermissionInt = int.Parse(userPermission);
                if (userPermissionInt < 3)
                {
                    return new Tuple<bool, object>(false, "You do not have a permission to update column");
                }
                var oldColumnName = updateColumnModel.oldColumnName;
                var newColumnName = updateColumnModel.newColumnName;
                var columnType = updateColumnModel.columnType;

                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                var commandString = $"ALTER TABLE laboratory_book_{laboratoryBookName} " +
                                    $"CHANGE COLUMN `{oldColumnName}` `{newColumnName}` {columnType} " +
                                    $"NULL DEFAULT NULL; ";

                LaboratoryBookHelper.DbNoQuery(connectionString, commandString);

                return new Tuple<bool, object>(true, "Column was successfully updated");
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }
        }

        [HttpPut]
        public Tuple<bool, object> UpdateUserPermission([FromBody]UpdateUserPermissionModel updateUserPermissionModel)
        {
            try
            {
                var userPermission = HttpContext
                  .User
                  .Claims
                  .First(claim => claim.Type == "Permission")
                  .Value;

                var userPermissionInt = int.Parse(userPermission);
                if (userPermissionInt < 3)
                {
                    return new Tuple<bool, object>(false, "You do not have a permission to remove list value");
                }

                var permissionId = updateUserPermissionModel.permissionId;
                var userName = updateUserPermissionModel.userName;

                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");
                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                var userId = LaboratoryBookHelper.GetUserId(connectionString, userName);

                var LaboratoryBookId = LaboratoryBookHelper.GetLaboratoryBookId(connectionString, laboratoryBookName);
                var commandString = "UPDATE `db_users` " +
                                    $"SET `permission_id` = '{permissionId}' " +
                                    $"WHERE (`user_id` = '{userId}') and (`db_id` = '{LaboratoryBookId}'); ";

                var permissionUpdateResult = LaboratoryBookHelper.DbNoQuery(connectionString, commandString);
                if (permissionUpdateResult > 0)
                {
                    return new Tuple<bool, object>(true, "Premission was successfully updated");
                }
                else
                {
                    return new Tuple<bool, object>(false, "Premission was not updated");
                }
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }

        }

        [HttpGet]
        public Tuple<bool, object> GetColumns()
        {
            try
            {
                var connectonString = _configuration
                              .GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;

                //default columns which are forbidden to delete or modify
                var ForbidenToModifyColumnNames = new string[]
                {
                "sampleID",
                "date",
                "material",
                "substrate",
                "thickness",
                "depositionTime",
                "depositionPressure",
                "regime",
                "description",
                "operator",
                "permissionID",
                };

                var commandString = $"SHOW columns FROM laboratory_book_{laboratoryBookName}; ";
                var columnsInfoTable = LaboratoryBookHelper.GetDbDataTable(connectonString, commandString);

                var columnInfoList = new List<object>();
                foreach (DataRow row in columnsInfoTable.Rows)
                {
                    if (ForbidenToModifyColumnNames.Contains(row[0].ToString())) continue;
                    columnInfoList.Add(new { columnName = row[0], columnType = row[1] });
                }

                return new Tuple<bool, object>(true, columnInfoList);
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }
        }

        [HttpGet]
        public Tuple<bool, object> GetPermissions()
        {
            try
            {
                var connectonString = _configuration
                              .GetConnectionString("LaboratoryBookConnectionString");
                var commandString = "SELECT permission_id FROM permission; ";
                var permissionIdDataTable = LaboratoryBookHelper.GetDbDataTable(
                    connectonString,
                    commandString);

                var userPermission = HttpContext
                  .User
                  .Claims
                  .First(claim => claim.Type == "Permission")
                  .Value;

                var userPermissionInt = int.Parse(userPermission);                

                var result = new List<object>();
                foreach (DataRow row in permissionIdDataTable.Rows)
                {
                    var permission = (int)((sbyte)row[0]);

                    if (permission < userPermissionInt)
                    {
                        result.Add(row[0]);
                    }
                }
                return new Tuple<bool, object>(true, result);
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception);
            }
        }

        [HttpGet]
        public Tuple<bool, object> GetUsersInfo()
        {
            try
            {
                var connectonString = _configuration
                          .GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook")
                    .Value;
                var commandStringBuilder = new StringBuilder();

                //get laboratory book id
                commandStringBuilder.Append("SELECT db_id ");
                commandStringBuilder.Append("FROM db_list ");
                commandStringBuilder.Append($"WHERE db_name = '{laboratoryBookName}'");

                var commandString = commandStringBuilder.ToString();

                var laboratoryBookId = LaboratoryBookHelper.GetDbDataScalar(
                    connectonString,
                    commandString);

                //get laboratory book users            
                commandStringBuilder.Clear();

                commandStringBuilder.Append("SELECT users.user_id, user_name, permission_id ");
                commandStringBuilder.Append("FROM users ");
                commandStringBuilder.Append("JOIN db_users ");
                commandStringBuilder.Append("ON users.user_id = db_users.user_id ");
                commandStringBuilder.Append($"WHERE db_users.db_id = '{laboratoryBookId}';");

                commandString = commandStringBuilder.ToString();

                var usersInfoDataTable = LaboratoryBookHelper.GetDbDataTable(
                    connectonString,
                    commandString);
                var userPermission = HttpContext
                 .User
                 .Claims
                 .First(claim => claim.Type == "Permission")
                 .Value;

                var userPermissionInt = int.Parse(userPermission);

                var usersInfoList = new List<object>();

                foreach (DataRow row in usersInfoDataTable.Rows)
                {
                    var permission =(int)((sbyte)row[2]);

                    if (userPermissionInt > permission)
                    {
                        usersInfoList.Add(new { userId = row[0], name = row[1], permissionId = row[2] });
                    }                   
                }
                return new Tuple<bool, object>(true, usersInfoList);
            }
            catch (Exception exception)
            {
                return new Tuple<bool, object>(false, exception.Message);
            }

        }

    }
}