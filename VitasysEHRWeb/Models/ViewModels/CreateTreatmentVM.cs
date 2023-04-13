using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class CreateTreatmentVM
    {
        public Treatment? Treatment { get; set; }
        public TreatmentRecord? TreatmentRecord { get; set; }
    }
}
