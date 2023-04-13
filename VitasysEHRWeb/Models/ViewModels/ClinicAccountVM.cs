using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class ClinicAccountVM
    {
        public Clinic? Clinic { get; set; }
        public Subscription? Subscription { get; set; }
        public string? BillingDate { get; set; }
        public bool IsDueForPayment { get; set; }
    }
}
