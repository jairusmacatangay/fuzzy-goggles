using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class InvoiceListVM
    {
        public int Id { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? InvoiceStatus { get; set; }
        public bool IsArchived { get; set; }
    }
}
