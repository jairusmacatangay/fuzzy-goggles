using Microsoft.AspNetCore.Identity;
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
    public class ApplicationUser : IdentityUser
    {
        [Required, Display(Name = "First Name")]
        [RegularExpression("[a-zA-Z]+ ?[a-zA-Z]*", ErrorMessage = "First name must not contain special characters or numbers")]
        [DataType(DataType.Text)]
        [MinLength(2, ErrorMessage = "First Name must be at least 2 letters")]
        public string? FirstName { get; set; }
        [Display(Name = "Middle Name")]
        [RegularExpression("[a-zA-Z]+ ?[a-zA-Z]*", ErrorMessage = "Middle name must not contain special characters or numbers")]
        [DataType(DataType.Text)]
        [MinLength(2, ErrorMessage = "Middle Name must be at least 2 letters")]
        public string? MiddleName { get; set; }
        [Required, Display(Name = "Last Name")]
        [RegularExpression("[a-zA-Z]+ ?[a-zA-Z]*", ErrorMessage = "Last name must not contain special characters or numbers")]
        [DataType(DataType.Text)]
        [MinLength(2, ErrorMessage = "Last Name must be at least 2 letters")]
        public string? LastName { get; set; }
        [Required]
        public string? Gender { get; set; }
        [Required, Display(Name = "Date of Birth")]
        public string? DOB { get; set; }
        [Required]
        [MinLength(2, ErrorMessage = "Address requires at least 2 letters")]
        public string? Address { get; set; }
        [Required]
        [MinLength(2, ErrorMessage = "City requires at least 2 letters")]
        public string? City { get; set; }
        [Required]
        [MinLength(2, ErrorMessage = "Province requires at least 2 letters")]
        public string? Province { get; set; }
        [Display(Name = "Zip Code")]
        [MaxLength(4, ErrorMessage = "Zip code must be 4 digits")]
        [MinLength(4, ErrorMessage = "Zip code must be 4 digits")]
        [RegularExpression(@"\d{4,4}", ErrorMessage = "Zip code must be a number")]
        [Required]
        public string? ZipCode { get; set; }

        public int? ClinicId { get; set; }
        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfPicUrl { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
        public bool IsArchived { get; set; }
        public string? Permit { get; set; }
        public string? AdminVerified { get; set; }

        public bool IsEditable { get; set; }

        public ICollection<TreatmentRecord>? TreatmentRecords { get; set; }
    }
}
