using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class FolderType
    {
        [Key]
        public int Id { get; set; }
        public string? Type { get; set; }
    }
}
