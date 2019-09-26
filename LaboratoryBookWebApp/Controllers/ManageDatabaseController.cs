using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaboratoryBookWebApp.Attributes;
using LaboratoryBookWebApp.Helpers;
using LaboratoryBookWebApp.Models;
using LaboratoryBookWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LaboratoryBookWebApp.Controllers
{
    [Authorize]
    [AjaxOnlyController]
    public class ManageDatabaseController : Controller
    {
        private readonly IConfiguration _configuration;

        public ManageDatabaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult AddListValue()
        {
            return PartialView("_AddListValuePartial");
        }
        [HttpGet]
        public IActionResult AddColumn()
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

                var commandString = $"SHOW columns FROM laboratory_book_{laboratoryBookName}; ";
                var columnsTable = LaboratoryBookHelper.GetDbDataTable(
                    connectonString,
                    commandString);

                var columnList = new List<string>();
                foreach (DataRow row in columnsTable.Rows)
                {
                    columnList.Add((string)row[0]);
                }
                return PartialView("_AddColumnPartial",columnList);
            }
            catch(Exception exception)
            {
                return Json($"Exception:{exception.Message}");
            }           
        }
        [HttpGet]
        public IActionResult AddUser()
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

                //get permisssions
                var commandString = "SELECT permission_id FROM permission; ";
                var permissionIdDataTable = LaboratoryBookHelper.GetDbDataTable(
                    connectonString,
                    commandString);               

                var permissionIdsList = new List<object>();
                foreach (DataRow row in permissionIdDataTable.Rows)
                {
                    permissionIdsList.Add(row[0]);
                }
                //get laboratory book id
                var laboratoryBookId = LaboratoryBookHelper.GetLaboratoryBookId(
                        connectonString,
                        laboratoryBookName
                    );

                //get users which have a permission to access the database
                var commandStringBuilder = new StringBuilder();

                commandStringBuilder.Append("SELECT user_name ");
                commandStringBuilder.Append("FROM users ");
                commandStringBuilder.Append("JOIN db_users ");
                commandStringBuilder.Append("ON users.user_id = db_users.user_id ");
                commandStringBuilder.Append($"WHERE db_users.db_id = '{laboratoryBookId}';");

                commandString = commandStringBuilder.ToString();
                var usersWithPermissionDataTabel = LaboratoryBookHelper.GetDbDataTable(
                    connectonString,
                    commandString
                    );
                var usersWithPermissionList = new List<string>();
                foreach (DataRow row in usersWithPermissionDataTabel.Rows)
                {
                    usersWithPermissionList.Add((string)row[0]);
                }
                //get available users
                commandString = "SELECT user_id, user_name FROM users;";

                var userInfoDataTable = LaboratoryBookHelper.GetDbDataTable(
                    connectonString,
                    commandString);
                
                var userInfoList = new List<ManageDatabaseUserInfoModel>();
                foreach(DataRow row in userInfoDataTable.Rows)
                {
                    if (usersWithPermissionList.Contains((string)row[1])) continue;
                    
                    userInfoList.Add(new ManageDatabaseUserInfoModel()
                    {
                        UserID = (int)row[0],
                        UserName = (string)row[1]
                    }); 
                }
                var addUserModel = new ManageDatabaseAddUserModel()
                {
                    PermissionIds = permissionIdsList,
                    Users = userInfoList
                };
                
                return PartialView("_AddUserPartial", addUserModel);
            }
            catch(Exception exception)
            {
                return Json(exception.Message);
            }                
        }
    }
}