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
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Time")]
        public int AppointmentTimeId { get; set; }

        [ForeignKey("AppointmentTimeId")]
        [ValidateNever]
        public AppointmentTime? AppointmentTime { get; set; }

        [Required]
        [Display(Name = "Clinic")]
        public int ClinicId { get; set; }

        [ForeignKey("ClinicId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        [ValidateNever]
        public Patient? Patient { get; set; }

        [Required]
        [Display(Name = "Purpose of Appointment")]
        [StringLength(280, ErrorMessage = "You have exceeded the 280-character limit.")]
        public string? Description { get; set; }

        [Display(Name = "Appointment Status")]
        public int AppointmentStatusId { get; set; }

        [ForeignKey("AppointmentStatusId")]
        [ValidateNever]
        public AppointmentStatus? AppointmentStatus { get; set; }

        [StringLength(280, ErrorMessage = "You have exceeded the 280-character limit.")]
        [MinLength(2, ErrorMessage ="Please input at least 2 characters")]
        public string? Notes { get; set; }

        public DateTime? LastModified { get; set; }
    }
}
