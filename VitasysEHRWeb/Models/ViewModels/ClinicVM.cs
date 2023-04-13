using Microsoft.AspNetCore.Mvc.Rendering;

namespace VitasysEHR.Models.ViewModels
{
    public class ClinicVM
    {
        public Clinic? Clinic { get; set; }

        public List<SelectListItem>? StartTimeList { get; set; }
        public List<SelectListItem>? EndTimeList { get; set; }
    }
}
