using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class DentalChartListVM
    {
        public int Id { get; set; }

        public string? Clinic { get; set; }
        public DateTime? EncounterDate { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
