using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.ViewModels
{
    public class MainPageModel
    {
        public DataTable LaboratoryBookData { get; set; }
        public string LaboratoryBookName { get; set; }
    }
}
