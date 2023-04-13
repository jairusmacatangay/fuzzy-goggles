using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class ToothVM
    {
        public int Id { get; set; }
        public int ToothNo { get; set; }
        public string? Type { get; set; }
        public ToothLabel? Condition { get; set; }
        public ToothLabel? BuccalOrLabial { get; set; }
        public ToothLabel? Lingual { get; set; }
        public ToothLabel? Mesial { get; set; }
        public ToothLabel? Distal { get; set; }
        public ToothLabel? Occlusal { get; set; }
    }
}
