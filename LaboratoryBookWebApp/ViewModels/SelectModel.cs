using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.ViewModels
{
    public class SelectModel
    {
        public IEnumerable<string> LaboratoryBooks { get; set; }
        public bool UserIsAdminister { get; set; } = false;
       
    }
}
