using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class UpdateColumnModel
    {
        public string oldColumnName { get; set; }
        public string newColumnName { get; set; }
        public string columnType { get; set; }
    }
}
