using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public  class MedicalHistoryVM
    {
        public int PatientId { get; set; }

        public string? Gender { get; set; }
        public List<SelectListItem>? BloodTypes { get; set; }

        public MedicalHistory MedicalHistory { get; set; }
        public Allergy Allergy { get; set; }
        public ReviewOfSystem ReviewOfSystem { get; set; }

    }
}
