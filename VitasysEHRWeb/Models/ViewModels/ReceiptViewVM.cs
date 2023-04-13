using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class ReceiptViewVM
    {
       
        public IEnumerable<Invoice> Invoices { get; set; }
        public Invoice Invoice { get; set; }
        public Clinic clinic { get; set; }
        public IEnumerable<TreatmentRecord> TreatmentRecords { get; set; }
        public decimal BalanceDue { get; set; }
        public string? ClinicName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? ZipCode { get; set; }
        public string? OfficePhone { get; set; }
        public string? MobilePhone { get; set; }
        public string? EmailAddress { get; set; }
        public string? PatientName { get; set; }
        public string? InvoiceDate { get; set; }
        public string? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
