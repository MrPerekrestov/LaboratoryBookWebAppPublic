using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class ManageUsersDeleteUserModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int UserStatusId { get; set; }
    }
}
