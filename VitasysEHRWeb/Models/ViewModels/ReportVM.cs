using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public  class ReportVM
    {
        public string ReportId { get; set; }
        public List<SelectListItem>? ReportList { get; set; }
    }
}
