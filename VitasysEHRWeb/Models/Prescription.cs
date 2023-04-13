using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }
        
        public int ClinicId { get; set; }
        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        [ValidateNever]
        public Patient? Patient { get; set; }

        [Required]
        public string? Drug { get; set; }
        [Required]
        public string? Dose { get; set; }
        [Required]
        public string? Dosage { get; set; }
        [Required]
        public string? Sig { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        [Required, Display(Name = "Quantity")]
        public int Quantity { get; set; }
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Other Notes")]
        public string? OtherNotes { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
        public bool IsArchived { get; set; }

    }
}
