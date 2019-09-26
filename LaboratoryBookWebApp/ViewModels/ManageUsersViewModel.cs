using LaboratoryBookWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.ViewModels
{
    public class ManageUsersViewModel
    {
        public List<ModifyUserModel> Users { get; set; }
        public List<AccessStatusModel> UserStatuses { get; set; }
    }
}
