using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class AccessStatusModel
    {
        private string accessName;

        public string AccessName
        {
            get
            {
                return accessName;
            }
            set
            {
                accessName = value;              
            }
        }

        private int accessId;

        public int AccessId
        {
            get
            {
                return accessId;
            }
            set
            {
                accessId = value;               
            }
        }

        public AccessStatusModel(int _accessId, string _accessName)
        {
            this.AccessId = _accessId;
            this.AccessName = _accessName;
        }
    }
}
