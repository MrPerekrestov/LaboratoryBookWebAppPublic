
using LaboratoryBookWebApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryBook.UserClass
{
    public interface IAdvancedUser
    {       
        List<ModifyUserModel> GetAvailableUsers(string connectionString);
        List<AccessStatusModel> GetAccessStatusList(string connectionString);
        bool ChangeUserStatus(string connectionString, ModifyUserModel changedUser);
        bool RemoveUser(string connectionString, int userId);
        int CreateUser(string connectionString, string userName, string password, int accessId);
    }
}
