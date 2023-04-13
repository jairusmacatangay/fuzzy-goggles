using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class TreatmentRecordVM
    {

        public TreatmentRecord? TreatmentRecord { get; set; }
        public int? InvoiceId { get; set; }
        public string? TreatmentName { get; set; }
        public string? ToothNumbers { get; set; }
        public string? ClinicName { get; set; }
        public string? Dentists { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public string? DateCreated { get; set; }
        public string? LastModified { get; set; }
    }
}
