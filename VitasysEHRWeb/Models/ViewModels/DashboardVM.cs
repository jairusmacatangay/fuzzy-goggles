using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class DashboardVM
    {
        public int PatientId { get; set; }
        public DentalChartVM? DentalChartVM { get; set; }
        public MedicalHistoryVM? MedicalHistoryVM { get; set; }
        public List<TreatmentRecord>? TreatmentRecords { get; set; }
        public List<Prescription>? Prescriptions { get; set; }
        public List<Folder>? Folders { get; set; }
        public List<Document>? Documents { get; set; }
        public List<Reminder>? Reminders { get; set; }
    }
}
