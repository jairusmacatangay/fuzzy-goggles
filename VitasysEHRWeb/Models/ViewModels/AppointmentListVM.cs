using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class AppointmentListVM
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public AppointmentTime? AppointmentTime { get; set; }
        public string? Clinic { get; set; }
        public string? AppointmentStatus { get; set; }
        public string? Patient { get; set; }
    }
}
