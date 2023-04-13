using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class ReceiptVM
    {
       
        public int Id { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? Clinic { get; set; }
        public string? PaymentStatus { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
