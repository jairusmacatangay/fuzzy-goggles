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
    public class Patient
    {
        [Key]
        public int Id { get; set; }
        [Required, Display(Name = "First Name")]
        [RegularExpression("[a-zA-Z\\s]+",ErrorMessage = "First name must not contain special characters or numbers")]
        public string? FirstName { get; set; }
        [Display(Name = "Middle Name")]
        [RegularExpression("[a-zA-Z\\s]+", ErrorMessage = "Middle name must not contain special characters or numbers")]
        public string? MiddleName { get; set; }
        [Required, Display(Name = "Last Name")]
        [RegularExpression("[a-zA-Z- Ññ\\s]+", ErrorMessage = "Last name must not contain special characters or numbers")]
        public string? LastName { get; set; }
        [Required, Display(Name = "Date of Birth")]
        public string? DOB { get; set; }
        [Required]
        public string? Gender { get; set; }
        [Required]
        public string? Address { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? Province { get; set; }
        [Required, Display(Name = "Zip Code")]
        [RegularExpression(@"\d{4,4}", ErrorMessage = "Zip code must be a 4-digit number")]
        public string? ZipCode { get; set; }
        public string? Password { get; set; }
        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Display(Name = "Home Number")]
        [RegularExpression("[()\\d-]+",ErrorMessage ="Invalid number format")]
        [DataType(DataType.PhoneNumber)]
        public string? HomeNumber { get; set; }
        [Required, Display(Name = "Mobile Number")]
        [RegularExpression("[()\\d-]+", ErrorMessage = "Invalid number format")]
        [DataType(DataType.PhoneNumber)]
        public string? MobileNumber { get; set; }
        public bool IsVerified { get; set; }
        public bool PortalAccess { get; set; }
        public bool PrivacyNotice { get; set; }
        [Display(Name = "Profile Picture")]
        public string? ProfPicUrl { get; set; }
        public bool IsArchived { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }

        public bool IsEditable { get; set; }
        public ICollection<ClinicPatient>? ClinicPatients { get; set; }
    }
}
