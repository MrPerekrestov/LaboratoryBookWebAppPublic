using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.ViewModels
{
    public class AddRowModel
    {
        public IEnumerable<string> Regimes { get; set; }
        public IEnumerable<string> Operators { get; set; }
        public IEnumerable<string> Materials { get; set; }
        public IEnumerable<string> Substrates { get; set; }
        public IEnumerable<string> PermissionIDs { get; set; }
        public IEnumerable<string> Columns { get; set; }
        public int RowId { get; set; }

    }
}
