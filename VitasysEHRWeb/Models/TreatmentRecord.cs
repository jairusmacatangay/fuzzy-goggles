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
    public class TreatmentRecord
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        [ValidateNever]
        public Patient? Patient { get; set; }

        public int? TreatmentId { get; set; }
        [ForeignKey("TreatmentId")]
        [ValidateNever]
        public Treatment? Treatment { get; set; }

        public int? InvoiceId { get; set; }
        [ForeignKey("InvoiceId")]
        [ValidateNever]
        public Invoice? Invoice { get; set; }

        public DateTime? DateCreated { get; set; }
        public DateTime? LastModified { get; set; }

        public ICollection<ApplicationUser>? ApplicationUsers { get; set; }

        public string? ToothNumbers { get; set; }
        public string? Dentists { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }

        public bool IsArchived { get; set; }

        public int ClinicId { get; set; }

        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }
    }
}
