using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class ModifyUserModel
    {
        private string _userName;

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;                
            }
        }

        private int _userStatusId;

        public int UserStatusId
        {
            get
            {
                return _userStatusId;
            }
            set
            {
                _userStatusId = value;               
            }
        }

        public int UserId { get; set; } = 0;
        public ModifyUserModel()
        {

        }
        public ModifyUserModel(string userName, int userStatusId)
        {
            this.UserName = userName;
            this.UserStatusId = userStatusId;
        }

        public ModifyUserModel(int userId, string userName, int userStatusId)
        {
            this.UserId = userId;
            this.UserName = userName;
            this.UserStatusId = userStatusId;
        }
    }
}
