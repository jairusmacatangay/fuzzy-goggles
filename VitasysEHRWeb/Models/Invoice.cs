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
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime? InvoiceDate { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        [ValidateNever]
        public Patient? Patient { get; set; }

        public int ClinicId { get; set; }
        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Payment Status")]
        public int? PaymentStatusId { get; set; }
        [ForeignKey("PaymentStatusId")]
        [ValidateNever]
        public PaymentStatus? PaymentStatus { get; set; }

        [Display(Name = "Payment Method")]
        public int? PaymentMethodId { get; set; }
        [ForeignKey("PaymentMethodId")]
        [ValidateNever]
        public PaymentMethod? PaymentMethod { get; set; }

        public bool IsArchived { get; set; }
        public DateTime? LastModified { get; set; }

        public string? InvoiceStatus { get; set; }
    }
}
