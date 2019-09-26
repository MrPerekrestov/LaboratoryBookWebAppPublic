using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Helpers
{
    public class DeleteLaboratoryBookOptions
    {
        public string ConnectionString { get; set; }
        public string EmbeddedResourcePath { get; set; }
        public int LaboratoryBookId { get; set; }
        public int UserId { get; set; }
        public string LaboratoryBookName { get; set; }
    }
}
