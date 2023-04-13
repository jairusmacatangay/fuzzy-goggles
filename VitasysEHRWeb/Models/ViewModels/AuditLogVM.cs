using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class AuditLogVM
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Clinic { get; set; }
        public DateTime DateAdded { get; set; }
        public string? ActivityType { get; set; }
        public string? Description { get; set; }
        public string? Device { get; set; }
    }
}
