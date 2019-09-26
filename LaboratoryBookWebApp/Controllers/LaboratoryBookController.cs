using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using LaboratoryBookWebApp.ViewModels;
using LaboratoryBookWebApp.Helpers;
using Microsoft.Extensions.Configuration;
using LaboratoryBookWebApp.Enums;
using System.Data;
using Newtonsoft.Json;
using System.Text;
using System.Collections;
using LaboratoryBookWebApp.Models;
using Microsoft.AspNetCore.Http;
using LaboratoryBook.UserClass;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using LaboratoryBookWebApp.Attributes;

namespace LaboratoryBookWebApp.Controllers
{
    [Authorize]
    public class LaboratoryBookController : Controller
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public LaboratoryBookController(ILogger<LaboratoryBookController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        [HttpGet]
        [AjaxOnly]
        public IActionResult GetLaboratoryBookName()
        {
            try
            {
                var laboratoryBookName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "LaboratoryBook").Value;
                return Ok(new {LaboratoryBookName = laboratoryBookName });
            }
            catch(Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = exception.Message });
            }
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult SelectColumns()
        {
            try
            {
                var connectonString = _configuration
                   .GetConnectionString("LaboratoryBookConnectionString");
                var laboratoryBook = HttpContext.User.Claims.First(claim => claim.Type == "LaboratoryBook").Value;
                var commandString = $"SHOW COLUMNS FROM laboratory_book_{laboratoryBook}; ";

                var columnsDataTableResult = LaboratoryBookHelper.GetDbDataTable(
                    connectonString,
                    commandString);

                var columnList = new List<string>();

                foreach(DataRow row in columnsDataTableResult.Rows)
                {
                    columnList.Add((string)row[0]);
                }

                return PartialView("_SelectColumnsPartial",columnList);
            }
            catch(Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new {message = exception.Message});
            }            
        }
        [HttpGet]
        [Authorize(Roles = "Administer,Moderator")]
        [AjaxOnly]
        public IActionResult ManageUsers()
        {
            try
            {
                var connectonString = _configuration
                    .GetConnectionString("LaboratoryBookConnectionString");
                var laboratoryBook = HttpContext.User.Claims.First(claim => claim.Type == "LaboratoryBook").Value;

                var userStatus = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserStatus")
                    .Value;

                var userId = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserId")
                    .Value;

                var user = LaboratoryBookHelper.CurrentUser(userStatus);
                if (!(user is IAdvancedUser))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new { message = "You do not have a permission to manage users" });
                }
                var advancedUser = user as IAdvancedUser;

                var statusList = advancedUser.GetAccessStatusList(connectonString);
                var userList = advancedUser.GetAvailableUsers(connectonString);

                var currenUser = userList.Where(userInfo =>userInfo.UserId == int.Parse(userId))
                                         .First();

                userList.Remove(currenUser);

                var viewModel = new ManageUsersViewModel()
                {
                    Users = userList,
                    UserStatuses = statusList
                };

                return PartialView("_ManageUsersPartial", viewModel);
            }
            catch (Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = exception.Message });
            }

        }
        [HttpGet]
        [Authorize(Roles = "Administer,Moderator,Laborant")]
        [AjaxOnly]
        public IActionResult ModifyUser()
        {
            var userName = HttpContext
                .User
                .Claims
                .First(claim => claim.Type == "UserName")
                .Value;

            var userStatus = HttpContext
                .User
                .Claims
                .First(claim => claim.Type == "UserStatus")
                .Value;

            var modifyUserViewModel = new ModifyUserViewModel()
            {
                UserName = userName,
                UserStatus = userStatus
            };
            return PartialView("_ModifyUserPartial", modifyUserViewModel);
        }
        [HttpGet]
        [Authorize(Roles = "Administer,Moderator,Laborant")]
        [AjaxOnly]
        public IActionResult ManageDatabase()
        {
            return PartialView("_ManageDatabasePartial");
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult RefreshTable()
        {
            try
            {
                var connectonString = _configuration
                    .GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBook = HttpContext.User.Claims.First(claim => claim.Type == "LaboratoryBook").Value;

                var userId = int.Parse(HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserId")
                    .Value);

                var userStatus = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserStatus")
                    .Value;

                var permission = LaboratoryBookHelper.GetPermissionId(
                        connectonString,
                        laboratoryBook,
                        userId);

                var laboratoryBookData = LaboratoryBookHelper
                    .CurrentUser(userStatus)
                    .GetDataFromLaboratoryBook(
                        connectonString,
                        laboratoryBook,
                        permission);

                return PartialView("_TablePartial", laboratoryBookData);
            }
            catch (Exception exception)
            {
                return Json(new Tuple<bool, string>(false, exception.Message));
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administer,Moderator,Laborant")]
        [AjaxOnly]
        public IActionResult DeleteRowFromDatabase([FromBody] DeleteRowFromDatabaseModel deleteRowFromDatabaseModel)
        {
            var sampleID = deleteRowFromDatabaseModel.SampleID;
            var samplePermissionId = deleteRowFromDatabaseModel.SamplePermissionId;
            var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");

            var laboratoryBook = HttpContext
                .User
                .Claims
                .First(claim => claim.Type == "LaboratoryBook")
                .Value;
            try
            {
                var userPermission = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "Permission")
                    .Value;

                var userPermissionInt = int.Parse(userPermission);
                if ((userPermissionInt == 1)||(userPermissionInt< samplePermissionId))
                {
                    throw new Exception("You do not have a permission to delete sample");
                }                
                
                var commandString = $"DELETE FROM laboratory_book_{laboratoryBook} " +
                                    $"WHERE sampleID = {sampleID}; ";
                if (LaboratoryBookHelper.DbNoQuery(connectionString, commandString) > 0)
                {
                    return Json(new Tuple<bool, string>(true, "Row was successfully deleted"));
                }
                else
                {
                    throw new Exception("Row deleting error");
                }

            }
            catch (Exception exception)
            {
                return Json(new Tuple<bool, string>(false, exception.Message));
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administer,Moderator,Laborant")]
        [AjaxOnly]
        public IActionResult AddRowToDatabase([FromBody] AddRowToDatabaseModel addRowToDatabaseModel)
        {
            

            var rowData = addRowToDatabaseModel.rowData;
            var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");
            var laboratoryBook = HttpContext.User.Claims.First(claim => claim.Type == "LaboratoryBook").Value;
            try
            {
                var userPermission = HttpContext
                .User
                .Claims
                .First(claim => claim.Type == "Permission")
                .Value;

                if (userPermission == "1")
                {
                    throw new Exception("You do not have a permission to add row");
                }

                var rowDataDictionary = JsonConvert
                    .DeserializeObject<Dictionary<string, string>>(rowData);

                var commandString = $"INSERT INTO laboratory_book_{laboratoryBook} (";
                var valuesString = "VALUES(";
                var lastFieldValuePair = rowDataDictionary.Last();
                foreach (var keyValue in rowDataDictionary)
                {
                    if (keyValue.Key != lastFieldValuePair.Key)
                    {
                        commandString += keyValue.Key + ", ";
                        valuesString += "'" + keyValue.Value + "', ";
                    }
                    else
                    {
                        commandString += keyValue.Key + ") ";
                        valuesString += "'" + keyValue.Value + "'); ";
                    }

                }

                commandString += valuesString;

                if (LaboratoryBookHelper.DbNoQuery(connectionString, commandString) > 0)
                {
                    return Json(new Tuple<bool, string>(true, "Row was successfully added"));
                }
                else
                {
                    throw new Exception("Row addition error");
                }
            }
            catch (Exception exception)
            {
                return Json(new Tuple<bool, string>(false, exception.Message));
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administer,Moderator,Laborant")]
        [AjaxOnly]
        public async Task<IActionResult> AddRow()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");
                var laboratoryBook = HttpContext.User.Claims.First(claim => claim.Type == "LaboratoryBook").Value;

                var getRegimesTask = Task.Run<string>(() => LaboratoryBookHelper
                .GetColumnList(
                    connectionString,
                    laboratoryBook,
                    ColumnToGet.Regime));

                var getOperatorsTask = Task.Run<string>(() => LaboratoryBookHelper
                .GetColumnList(
                    connectionString,
                    laboratoryBook,
                    ColumnToGet.Operator));

                var getMaterialsTask = Task.Run<string>(() => LaboratoryBookHelper
                .GetColumnList(
                    connectionString,
                    laboratoryBook,
                    ColumnToGet.Material));

                var getSubstratesTask = Task.Run<string>(() => LaboratoryBookHelper
                .GetColumnList(
                    connectionString,
                    laboratoryBook,
                    ColumnToGet.Substrate));

                var getPermissionIDsTask = Task.Run<string>(() => LaboratoryBookHelper
                .GetColumnList(connectionString,
                laboratoryBook,
                ColumnToGet.PermissionID));

                var getColumnValuesTasks = new List<Task<string>>();

                getColumnValuesTasks.AddRange(new[] {
                    getRegimesTask,
                    getOperatorsTask,
                    getMaterialsTask,
                    getSubstratesTask,
                    getPermissionIDsTask});

                var addRowModel = new AddRowModel();

                while (getColumnValuesTasks.Any())
                {
                    var compleatedTask = await Task.WhenAny(getColumnValuesTasks.ToArray());

                    if (compleatedTask == getRegimesTask)
                    {
                        addRowModel.Regimes = compleatedTask.Result.Split(";");
                    }
                    else if (compleatedTask == getOperatorsTask)
                    {
                        addRowModel.Operators = compleatedTask.Result.Split(";");
                    }
                    else if (compleatedTask == getMaterialsTask)
                    {
                        addRowModel.Materials = compleatedTask.Result.Split(";");
                    }
                    else if (compleatedTask == getSubstratesTask)
                    {
                        addRowModel.Substrates = compleatedTask.Result.Split(";");
                    }
                    else
                    {
                        var userPermission = HttpContext
                            .User
                            .Claims
                            .First(claim => claim.Type == "Permission")
                            .Value;

                        var userPermissionInt = int.Parse(userPermission);
                        
                        addRowModel.PermissionIDs = compleatedTask
                            .Result
                            .Split(";")
                            .Where(permissionId=> {
                                var permissionIdInt = int.Parse(permissionId);
                                return permissionIdInt <= userPermissionInt;
                            });                        
                    }
                    getColumnValuesTasks.Remove(compleatedTask);
                }

                var maxRowId = LaboratoryBookHelper.GetDbDataScalar(
                    connectionString,
                    $"SELECT MAX(sampleID) FROM laboratory_book_{laboratoryBook}; "
                    );

                if (maxRowId is DBNull)
                {
                    addRowModel.RowId = 1;
                }
                else
                {
                    addRowModel.RowId = ((int)maxRowId) + 1;
                }

                var bookInfoDataTable = LaboratoryBookHelper.GetDbDataTable(
                    connectionString,
                    $"SHOW COLUMNS FROM laboratory_book_{laboratoryBook}; "
                    );

                var columnNames = new List<string>();
                foreach (DataRow dataRow in bookInfoDataTable.Rows)
                {
                    columnNames.Add((string)dataRow["Field"]);
                }
                addRowModel.Columns = columnNames.ToArray();
                return PartialView("_AddRowPartial", addRowModel);

            }
            catch (Exception exception)
            {
                return new ObjectResult(exception.Message);
            }
        }

        [HttpGet]
        [Route("LaboratoryBook/MainPage/{laboratoryBook}")]        
        public async Task<IActionResult> MainPage(string laboratoryBook)
        {
            var connectonString = _configuration.GetConnectionString("LaboratoryBookConnectionString");

            try
            {
                var userId = int.Parse(HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserId")
                    .Value);

                var userStatus = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserStatus")
                    .Value;

                var permission = LaboratoryBookHelper.GetPermissionId(
                    connectonString,
                    laboratoryBook,
                    userId);

                var laboratoryBookData = LaboratoryBookHelper.CurrentUser(userStatus).GetDataFromLaboratoryBook(
                    connectonString,
                    laboratoryBook,
                    permission);

                var MainPageViewModel = new MainPageModel()
                {
                    LaboratoryBookData = laboratoryBookData,
                    LaboratoryBookName = laboratoryBook
                };               

                var claims = new List<Claim>
                {
                    new Claim("LaboratoryBook", laboratoryBook),
                    new Claim("Permission", permission.ToString())
                };
                claims.AddRange(HttpContext.User.Claims);

                var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(userIdentity);

                await HttpContext.SignOutAsync();
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return View(MainPageViewModel);
            }
            catch
            {
                return RedirectToAction("Authentication", "Login");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administer,Moderator,Laborant")]
        [AjaxOnly]
        public IActionResult UpdateValue([FromBody] UpdateValueModel updateValueModel)
        {
            try
            {
                var userPermission = HttpContext
               .User
               .Claims
               .First(claim => claim.Type == "Permission")
               .Value;

                if (userPermission == "1")
                {
                    throw new Exception("You do not have a permission to change laboratory book");
                }

                var dataId = updateValueModel.dataId;
                var dataType = updateValueModel.dataType;
                var dataValue = updateValueModel.dataValue;

                var connectonString = _configuration.GetConnectionString("LaboratoryBookConnectionString");
               
                var laboratoryBook = HttpContext.User.Claims.First(claim => claim.Type == "LaboratoryBook").Value;                

                var changeDatumResult = LaboratoryBookHelper.ChangeDatum(
                    connectonString,
                    laboratoryBook,
                    dataType,
                    dataValue,
                    dataId);

                if (!changeDatumResult)
                {
                    return Json(new Tuple<bool, string>(false, "datum was no changed"));
                }

                return Json(new Tuple<bool, string>(true, "success!"));

            }
            catch (Exception exception)
            {
                return Json(new Tuple<bool, string>(false, exception.Message));
            }

        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Getlist(string listName)
        {
            var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");

            var laboratoryBook = HttpContext.User.Claims.First(claim => claim.Type == "LaboratoryBook").Value;

            var columnToGet = new ColumnToGet();
            switch (listName)
            {
                case "regimes":
                    columnToGet = ColumnToGet.Regime;
                    break;
                case "operators":
                    columnToGet = ColumnToGet.Operator;
                    break;
                case "materials":
                    columnToGet = ColumnToGet.Material;
                    break;
                case "substrates":
                    columnToGet = ColumnToGet.Substrate;
                    break;
                case "permissionIDs":
                    columnToGet = ColumnToGet.PermissionID;
                    break;
                default: return null;
            }
            var columnList = LaboratoryBookHelper.GetColumnList(connectionString, laboratoryBook, columnToGet);
            return Content(columnList);
        }
    }
}