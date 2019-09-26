using LaboratoryBookWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.ViewModels
{
    public class ManageDatabaseAddUserModel
    {
        public List<object> PermissionIds { get; set; }
        public List<ManageDatabaseUserInfoModel> Users { get; set; }
    }
}
