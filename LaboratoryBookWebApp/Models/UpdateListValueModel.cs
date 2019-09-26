using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class UpdateListValueModel
    {
        public string listName { get; set; }
        public string oldListValue { get; set; }
        public string newListValue { get; set; }
    }
}
