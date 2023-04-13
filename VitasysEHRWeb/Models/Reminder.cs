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
    public class Reminder
    {
        [Key]
        public int Id { get; set; }

        public int ClinicId { get; set; }

        [ValidateNever]
        [ForeignKey("ClinicId")]
        public Clinic? Clinic { get; set; }

        public int PatientId { get; set; }

        [ValidateNever]
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public DateTime? LastModified { get; set; }
    }
}
