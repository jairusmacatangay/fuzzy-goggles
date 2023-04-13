using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class Allergy
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Local Anesthetic")]
        public bool IsAnesthesia { get; set; }
        [Display(Name = "Penicillin, Antibiotics")]
        public bool IsAntibiotics { get; set; }
        [Display(Name = "Sulfa Drugs")]
        public bool IsSulfa { get; set; }
        [Display(Name = "Aspirin")]
        public bool IsAspirin { get; set; }
        [Display(Name = "Latex")]
        public bool IsLatex { get; set; }
        public string? Other { get; set; }

    }
}
