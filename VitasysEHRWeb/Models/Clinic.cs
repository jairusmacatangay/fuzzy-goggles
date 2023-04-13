using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class Clinic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [RegularExpression("[a-zA-Z0-9 ]*", ErrorMessage ="Please use alphanumeric characters and space only")]
        [Display(Name = "Clinic Name")]
        public string? Name { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        [RegularExpression(@"[a-zA-Z]{1,15} ?[a-zA-Z]{1,15}",ErrorMessage ="Please do not include numbers or special characters")]
        public string? City { get; set; }

        [Required]
        [RegularExpression(@"\D{1,15} ?(\d{1,2}|\w{1,4})", ErrorMessage ="Please do not include special characters")]
        public string? Province { get; set; }

        [Required, Display(Name = "Zip Code")]
        [RegularExpression(@"^([0-9]{4})$",ErrorMessage ="The Zip Code field must be 4-digit numbers.")]
        public string? ZipCode { get; set; }

        [Display(Name = "Office Phone")]
        [RegularExpression(@"^\(\d{2,3}\)\d{3}-\d{4}",ErrorMessage = "Please follow the format: (02)123-4567")]
        public string? OfficePhone { get; set; }

        [Required, Display(Name = "Mobile Phone")]
        [RegularExpression(@"^(09|\+639)\d{9}$",ErrorMessage = "Please follow the format: 09XXXXXXXXX or +639XXXXXXXXX")]
        public string? MobilePhone { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        public string? EmailAddress { get; set; }

        [Display(Name = "Logo")]
        public string? LogoUrl { get; set; }
        
        public bool IsVerified { get; set; }
        public bool IsArchived { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public string? StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public string? EndTime { get; set; }


        public ICollection<ClinicPatient>? ClinicPatients { get; set; }
    }
}
