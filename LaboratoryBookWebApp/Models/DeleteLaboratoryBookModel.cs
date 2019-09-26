using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryBookWebApp.Models
{
    public class DeleteLaboratoryBookModel
    {
        [Required]
        public string LaboratoryBookName { get; set; }
    }
}
