using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class ReminderListVM
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Clinic { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
