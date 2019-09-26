using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LaboratoryBook.UserClass
{
    public class Guest : User
    {
        public Guest(string userName, int userID)
        {
            this.UserName = userName;
            this.AccessID = 1;
            this.UserID = userID;
        }
        public Guest()
        {

        }
        public override DataTable GetDataFromLaboratoryBook(string connectionString, string LaboratoryBookName, int Permission)
        {
            var dt = base.GetDataFromLaboratoryBook(connectionString, LaboratoryBookName, Permission);
            for(var i=0; i<dt.Columns.Count; i++)
            {
                dt.Columns[i].ReadOnly = true;
            }
            return dt;
        }
    }
}
