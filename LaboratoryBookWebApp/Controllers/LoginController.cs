using System;
using LaboratoryBookWebApp.Attributes;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LaboratoryBook.UserClass;
using LaboratoryBookWebApp.Enums;
using LaboratoryBookWebApp.Helpers;
using LaboratoryBookWebApp.Models;
using LaboratoryBookWebApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LaboratoryBookWebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public LoginController(IConfiguration configuration, IHostingEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }
        [HttpGet]
        public IActionResult Test()
        {
            return Json(_env.EnvironmentName);
        }
        [HttpDelete]
        [Authorize(Roles = "Administer")]
        [AjaxOnly]
        public async Task<IActionResult> DeleteLaboratoryBook([FromBody]DeleteLaboratoryBookModel deleteModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errorMessage = "Model state is invalid" });
            }
            try
            {
                var connectionString = _configuration
                       .GetConnectionString("LaboratoryBookConnectionString");

                var userId = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserId")
                    .Value;

                var creatorId = await LaboratoryBookHelper.GetLaboratoryBookCreatorAsync(
                        connectionString,
                        deleteModel.LaboratoryBookName
                    );

                if (creatorId != int.Parse(userId))
                {
                    return StatusCode(
                                StatusCodes.Status403Forbidden,
                                new {errorMessage = "You are not creator of this laboratory book"}
                            );
                }

                var userStatus = HttpContext
                   .User
                   .Claims
                   .First(claim => claim.Type == "UserStatus")
                   .Value;
                var user = (Administer)LaboratoryBookHelper.CurrentUser(userStatus);
               
                var deleteLaboratoryBookOptions = new DeleteLaboratoryBookOptions
                {
                    ConnectionString = connectionString,
                    EmbeddedResourcePath = "LaboratoryBookWebApp.TextFiles.DeleteLaboratoryBookTemplate.txt",
                    LaboratoryBookId = LaboratoryBookHelper.GetLaboratoryBookId(
                        connectionString,
                        deleteModel.LaboratoryBookName),
                    LaboratoryBookName = deleteModel.LaboratoryBookName,
                    UserId = int.Parse(userId)
                };

                await user.DeleteLaboratryBookAsync(deleteLaboratoryBookOptions);

                return Ok();
            }
            catch (Exception exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { errorMessage = exception.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administer")]
        [AjaxOnly]
        public async Task<IActionResult> CreateLaboratoryBook([FromBody]CreateLaboratoryBookModel createLaboratoryBookModel)
        {
            try
            {
                var connectonString = _configuration
                    .GetConnectionString("LaboratoryBookConnectionString");

                var laboratoryBookName = createLaboratoryBookModel.LaboratoryBookName;

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

                var userName = HttpContext
                    .User
                    .Claims
                    .First(claim => claim.Type == "UserName")
                    .Value;


                var createLaboratoryBookOptions = new CreateLaboratoryBookOptions
                {
                    ConnectionString = connectonString,
                    LaboratoryBookName = laboratoryBookName,
                    UserId = userId,
                    UserName = userName
                };

                var user = (Administer)LaboratoryBookHelper.CurrentUser(userStatus);

                var result = await user.CreateLaboratoryBook(createLaboratoryBookOptions);

                if (result)
                {
                    return Ok(new { message = $"Laboratory book {laboratoryBookName} was created" });
                }
                else
                {
                    throw new Exception("Laboratory book was not created");
                }

            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = exception.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> Authentication()
        {
            await HttpContext.SignOutAsync();
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Authentication(LoginModel model)
        {
            var conStr = "";
            try
            {
                if (ModelState.IsValid)
                {
                    var userName = model.UserName;
                    var password = model.Password;
                    conStr = _configuration.GetConnectionString("LaboratoryBookConnectionString");

                    var validationResult = LoginHelper.ValidateLoginAndPassword(
                        userName,
                        password,
                        conStr
                        );

                    if (validationResult.Item1)
                    {
                        var userId = validationResult.Item3.UserId;
                        var userStatus = validationResult.Item3.UserStatus;

                        var claims = new List<Claim>()
                    {
                        new Claim("UserId",userId.ToString()),
                        new Claim("UserName", userName),
                        new Claim("UserStatus",userStatus),
                        new Claim(ClaimTypes.Role, userStatus)
                    };

                        var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(userIdentity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        var bookList = SelectHelper.GetAvailableLaboratoryBooks(userId, conStr);
                        var selectViewModel = new SelectModel()
                        {
                            LaboratoryBooks = bookList
                        };
                        if (userStatus == "Administer")
                        {
                            selectViewModel.UserIsAdminister = true;
                        }
                        return PartialView("Select", selectViewModel);
                    }
                    else
                    {
                        throw new Exception(validationResult.Item2);
                    }
                }
                throw new Exception("Model state is not valid");
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = exception.Message                    
                });
            }
        }
        [Authorize]
        [HttpGet]
        [AjaxOnly]
        public IActionResult Select()
        {
            var conStr = _configuration.GetConnectionString("LaboratoryBookConnectionString");

            var userIdString = HttpContext.User.Claims.First(claim => claim.Type == "UserId").Value;
            var userId = int.Parse(userIdString);

            var bookList = SelectHelper.GetAvailableLaboratoryBooks(userId, conStr);

            var selectViewModel = new SelectModel()
            {
                LaboratoryBooks = bookList
            };
            return PartialView(selectViewModel);
        }
    }
}
