using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class ToothLabel
    {
        [Key]
        public int Id { get; set; }
        public string? Label { get; set; }
        public string? Abbreviation { get; set; }
        public string? Scope { get; set; }
        public string? Color { get; set; }
    }
}
