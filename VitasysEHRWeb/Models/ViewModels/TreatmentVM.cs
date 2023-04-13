using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class TreatmentVM
    {
        public Treatment? Treatment { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? TreatmentTypeList { get; set; }

        public Clinic? Clinic { get; set; }
        public int ClinicId { get; set; }
    }
}
