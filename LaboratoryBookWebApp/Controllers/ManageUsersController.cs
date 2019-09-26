using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LaboratoryBook.UserClass;
using LaboratoryBookWebApp.Attributes;
using LaboratoryBookWebApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LaboratoryBookWebApp.Controllers
{
    [Authorize]
    [AjaxOnlyController]
    public class ManageUsersController : Controller
    {
        private readonly IConfiguration _configuration;

        public ManageUsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult CreateUser()
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
                        new { message = "You do not have a permission to create a user" });
                }
                var advancedUser = user as IAdvancedUser;
                var statusList = advancedUser.GetAccessStatusList(connectonString);
                return PartialView("_CreateUserPartial",statusList);
            }
            catch(Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = exception.Message });
            }
        }
    }
}