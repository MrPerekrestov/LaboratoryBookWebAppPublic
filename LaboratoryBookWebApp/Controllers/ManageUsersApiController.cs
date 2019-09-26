using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LaboratoryBook.UserClass;
using LaboratoryBookWebApp.Attributes;
using LaboratoryBookWebApp.Enums;
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
    public class ManageUsersApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ManageUsersApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
      
        [HttpPut]
        public IActionResult UpdateUserStatus([FromBody]ModifyUserModel userInfo)
        {
            try
            {     
                var connectonString = _configuration
                    .GetConnectionString("LaboratoryBookConnectionString");
                var userStatus = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserStatus").Value;
                var user = LaboratoryBookHelper.CurrentUser(userStatus);

                if (!(user is IAdvancedUser))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new { message = "You do not have a permission to change the user satatus" });
                }

                var advancedUser = user as IAdvancedUser;
                var updateUserStatusResult = advancedUser.ChangeUserStatus(
                    connectonString,
                    userInfo);

                if (updateUserStatusResult)
                {
                    return Ok(new { message = $"{userInfo.UserName} status was updated to {userInfo.UserStatusId} " });
                }
                return BadRequest(new { message = $"{userInfo.UserName} status was not updated" });

            }
            catch(Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                     new {message = exception.Message });
            }
        }
        [HttpPost]
        public IActionResult CreateUser([FromBody]ManageUsersCreateUserModel newUserInfo)
        {
            try
            {
                if (newUserInfo.Password.Length<7)
                {
                    return BadRequest(new { message = "Password should be longer than 7 characters" });
                }
                var userNamePattern = new Regex(@"^[a-zA-Z0-9]{3,20}$");
                if (!userNamePattern.IsMatch(newUserInfo.UserName))
                {
                    return BadRequest(new { message = "User name should contain from 3 to 20 alphanumeric characters." });
                }

                var connectonString = _configuration
                    .GetConnectionString("LaboratoryBookConnectionString");
                var userStatus = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserStatus").Value;
                var user = LaboratoryBookHelper.CurrentUser(userStatus);

                if (!(user is IAdvancedUser))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new { message = "You do not have a permission to change the user satatus" });
                }
                var userIdStr = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserId").Value;
                user.UserID = int.Parse(userIdStr);

                var advancedUser = user as IAdvancedUser;
                var createUserResult = advancedUser.CreateUser(
                    connectonString,
                    newUserInfo.UserName,
                    newUserInfo.Password,
                    newUserInfo.Status);

                if (createUserResult == 0)
                {
                    throw new Exception("For some reason user was not created");
                }

                var newUserId = LaboratoryBookHelper.GetUserId(
                    connectonString,
                    newUserInfo.UserName);

                return Ok(newUserId);
            }
            catch(Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = exception.Message });
            }
        }        
        [HttpPost]
        public IActionResult DeleteUser([FromBody]ManageUsersDeleteUserModel deleteUserInfo)
        {
            try
            {
                //check user permission
                var userStatusStr = HttpContext.User.Claims.First(claim => claim.Type == "UserStatus").Value;

                var statusParseResult = Enum.TryParse(
                    typeof(UserStatus),
                    userStatusStr,
                    true,
                    out object userStatus);

                if (!statusParseResult)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "User status parse error" });
                }

                var userStatusId = (int)userStatus;

                if (userStatusId<=deleteUserInfo.UserStatusId)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new { message = "You do not have a permission to delete this user" });
                }
                //actual user deleting
                var user = LaboratoryBookHelper.CurrentUser(userStatusStr);
                var advancedUser = user as IAdvancedUser;
                var connectonString = _configuration
                   .GetConnectionString("LaboratoryBookConnectionString");

                var deleteUserResult = advancedUser.RemoveUser(
                    connectonString,
                    deleteUserInfo.UserId);

                if(deleteUserResult)
                {
                    return Ok(
                        new
                        {
                            message = $"User {deleteUserInfo.UserName} was successfully deleted."
                        });
                }
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "User was not deleted for some reason :("
                    });
                                       
            }
            catch(Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = exception.Message });
            }
        }
    }
}