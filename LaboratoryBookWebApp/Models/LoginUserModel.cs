using LaboratoryBookWebApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class LoginUserModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserStatus { get; set; }
    }
}
