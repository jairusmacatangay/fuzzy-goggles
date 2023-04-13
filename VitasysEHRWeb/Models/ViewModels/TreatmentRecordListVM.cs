using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class TreatmentRecordListVM
    {
        public int TreatmentRecordId { get; set; }
        public int? InvoiceID { get; set; }
        public string? TreatmentName { get; set; }
        public string? ToothNumbers { get; set; }
        public string? Dentists { get; set; }
        public decimal? TotalPrice { get; set; }
        public int Quantity { get; set; }
    }
}
