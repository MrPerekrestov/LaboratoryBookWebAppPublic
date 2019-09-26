using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LaboratoryBookWebApp.Attributes;
using LaboratoryBookWebApp.Helpers;
using LaboratoryBookWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    public class ModifyUserApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ModifyUserApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        } 
        [HttpPost]     
        public IActionResult ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {                
                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");
            
                var userIdstr = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserId").Value;
                var userId = int.Parse(userIdstr);

                var checkOldPasswordResult =  Password.CheckPassword(
                    connectionString,
                    userId,
                    model.oldPassword);

                if (!checkOldPasswordResult.Item1)
                {
                    return BadRequest(new { message = checkOldPasswordResult.Item2 });
                }

                var setNewPasswordResult = Password.SetNewPassword(
                    connectionString,
                    userId,
                    model.newPassword);

                if (setNewPasswordResult)
                {
                    return Ok(new {message = "Password changed" });
                }
                else
                {
                    return BadRequest(new { message = "Password was not changed" });
                }               

            }
            catch(Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = exception.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ModifyUserChangeNameAsync([FromBody]ChangeNameModel model)        
        {
            try
            {
                var newName = model.newName;               
                var namePattern = new Regex(@"^[a-zA-Z0-9]{3,20}$");
                if (!namePattern.IsMatch(newName))
                {
                    return BadRequest(new { message = "New name should contain from 3 to 20 aplhanumeric symbols" });                 
                }
                var connectionString = _configuration.GetConnectionString("LaboratoryBookConnectionString");             
                
                var userId = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserId")
                    .Value;

                var commandString = $"UPDATE `users` SET `user_name` = '{newName}' " +
                                   $"WHERE (`user_id` = '{userId}'); ";

                var userNameUpdateResult = LaboratoryBookHelper.DbNoQuery(
                    connectionString,
                    commandString);
                if (userNameUpdateResult > 0)
                {
                    var userStatus = HttpContext
                        .User
                        .Claims
                        .First(claim => claim.Type == "UserStatus")
                        .Value;

                    var userName = newName;

                    await HttpContext.SignOutAsync();

                    var claimsNoUserName = HttpContext.User.Claims.Where(claim => claim.Type != "UserName");
                    var claims = new List<Claim>()
                    {                        
                        new Claim("UserName", userName)                       
                    };

                    claims.AddRange(claimsNoUserName);
                    var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(userIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return Ok();
                }
                else
                {
                    return NotFound("User name was not updated");
                }

            }
            catch (Exception exception)
            {                
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = exception.Message });
            }
            
        }
    }
}