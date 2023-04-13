using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class CollectPaymentVM
    {
        public Invoice? Invoice { get; set; }
        public IEnumerable<SelectListItem>? PaymentMethods { get; set; }
    }
}
