using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Helpers
{
    public class CreateLaboratoryBookOptions
    {
        public string ConnectionString { get; set; }
        public string LaboratoryBookName { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
    }
}
