using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class PrescriptionFormVM
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        [Required]
        public string? Drug { get; set; }
        [Required]
        [RegularExpression("[a-zA-Z0-9 ]*", ErrorMessage = "Please do not use symbols.")]
        public string? Dose { get; set; }
        [Required]
        [RegularExpression("[a-zA-Z0-9 ]*", ErrorMessage = "Please do not use symbols.")]
        public string? Dosage { get; set; }
        [Required]
        public string? Sig { get; set; }
        [Required, Display(Name = "Quantity")]
        public int Quantity { get; set; }
        
        [Display(Name = "Other Notes")]
        public string? OtherNotes { get; set; }
        [Display(Name = "Start Date")]
        public string? StartDate { get; set; }
        [Display(Name = "End Date")]
        public string? EndDate { get; set; }
    }
}
