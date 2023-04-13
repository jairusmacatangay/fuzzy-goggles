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
    public class AuditLogPatient
    {
        [Key]
        public int Id { get; set; }

        public int? PatientId { get; set; }
        [ForeignKey("PatientId")]
        [ValidateNever]
        public Patient? Patient { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string? ActivityType { get; set; }
        public string? Description { get; set; }
        public string? Device { get; set; }
    }
}
