using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class AppointmentVM
    {
        public Appointment? Appointment { get; set; }

        [Required]
        [Display(Name = "Date")]
        public string? AppointmentDate { get; set; }

        public string? LastModified { get; set; }
        public IEnumerable<SelectListItem>? TimeSlots { get; set; }
        public IEnumerable<SelectListItem>? Clinics { get; set; }
        public List<string>? AvailableTimeslots { get; set; }
        public string? Patient { get; set; }
        public string? Clinic { get; set; }
    }
}
