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
    class Laborant : User
    {
       
        public Laborant(string userName, int userID)
        {
            this.UserName = userName;
            this.AccessID = 2;
            this.UserID = userID;
        }     
        public Laborant()
        {

        }

    }
}
