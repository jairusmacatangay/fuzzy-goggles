using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class ToothFormVM
    {
        public Tooth? Tooth { get; set; }
        public List<SelectListItem>? LabelWholeList { get; set; }
        public List<SelectListItem>? LabelPartList { get; set; }
    }
}
