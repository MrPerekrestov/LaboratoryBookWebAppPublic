using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class UpdateValueModel
    {
        public int dataId { get; set; }
        public string dataType { get; set; }
        public string dataValue { get; set; }
    }
}
