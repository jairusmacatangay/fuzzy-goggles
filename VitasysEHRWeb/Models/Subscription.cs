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
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        public int ClinicId { get; set; }
        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        public string? Type { get; set; }
        public string? PaymentMode { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsLockout { get; set; }
    }
}
