using LaboratoryBookWebApp.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Attributes
{
    public class AjaxOnlyControllerAttribute: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.IsAjaxRequest())
            {
                context.Result = new BadRequestObjectResult(new {message = "Only ajax requests are allowed"});
            }            
        }
    }
}
