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
    public class Folder
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        [ValidateNever]
        public Patient? Patient { get; set; }
        [Display(Name="Folder Type")]
        public int FolderTypeId { get; set; }
        [ForeignKey("FolderTypeId")]
        [ValidateNever]
        public FolderType? FolderType { get; set; }
        [Required, Display(Name="Folder Name")]
        [RegularExpression(@"^[\w\- ]+$", ErrorMessage = "Folder name should only contain alphanumeric numbers, dash or underscore")]
        public string? Name { get; set; }
        public int? ClinicId { get; set; }
        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
    }
}
