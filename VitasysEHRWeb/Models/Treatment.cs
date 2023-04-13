using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class Treatment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage ="The name cannot exceed 50 characters.")]
        public string? Name { get; set; }
        [DisplayName("Treatment Type")]
        public int TreatmentTypeId { get; set; }
        [ForeignKey("TreatmentTypeId")]
        [ValidateNever]
        public TreatmentType? TreatmentType { get; set; }
        public int ClinicId { get; set; }
        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        [Required]
        [Range(0.1,999999.99, ErrorMessage = "The price must be greater than 0.00 and less than 1,000,000.00")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$",ErrorMessage ="The price must have at most 2 decimal places.")]
        public decimal Price { get; set; }
        public bool IsArchived { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
    }
}
