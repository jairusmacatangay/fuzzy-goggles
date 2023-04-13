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
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int SubscriptionId { get; set; }
        [ForeignKey("SubscriptionId")]
        [ValidateNever]
        public Subscription? Subscription { get; set; }

        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
    }
}
