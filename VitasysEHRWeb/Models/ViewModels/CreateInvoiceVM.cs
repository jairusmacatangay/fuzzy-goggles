using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class CreateInvoiceVM
    {
        public int InvoiceId { get; set; }
        public List<TreatmentRecordListVM>? TreatmentRecordList { get; set; }
        public List<TreatmentRecord>? UntaggedTreatmentRecordList { get; set; }
        public decimal TotalAmount { get; set; }

        public string? PatientName { get; set; }
    }
}
