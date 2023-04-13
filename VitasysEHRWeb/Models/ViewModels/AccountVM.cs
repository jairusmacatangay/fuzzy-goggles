using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class AccountVM
    {
        public ApplicationUser? ApplicationUser { get; set; }

        [Required, Display(Name = "Role")]
        public List<string>? SelectedRoles { get; set; }
        public List<SelectListItem>? RoleList { get; set; }

        public IEnumerable<SelectListItem>? Clinic { get; set; }

        [Required]
        public string Password { get; set; }
        [Required, Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match.")]
        public string ConfirmPassword { get; set; }
    }
}