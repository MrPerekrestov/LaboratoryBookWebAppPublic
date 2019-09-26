using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LaboratoryBookWebApp.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LaboratoryBookWebApp.Controllers
{
    [Authorize]
    [AjaxOnlyController]
    public class ModifyUserController : Controller
    {
        private readonly IConfiguration _configuration;

        public ModifyUserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]       
        public IActionResult ChangeUserName()
        {
            return PartialView("_ChangeUserNamePartial");
        }
    }
}
