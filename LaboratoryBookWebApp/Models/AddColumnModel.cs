using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class AddColumnModel
    {
        public string columnName { get; set; }
        public string columnType { get; set; }
        public string afterColumn { get; set; }
    }
}
