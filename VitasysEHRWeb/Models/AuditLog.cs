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
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        public int? ClinicId { get; set; }
        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string? ActivityType { get; set; }
        public string? Description { get; set; }
        public string? Device { get; set; }
    }
}
