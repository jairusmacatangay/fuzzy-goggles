using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class OralCavity
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Gingivitis")]
        public bool IsGingivitis { get; set; }
        [Display(Name = "Early Periodontitis")]
        public bool IsEarlyPerio { get; set; }
        [Display(Name = "Moderate Periodontitis")]
        public bool IsModPerio { get; set; }
        [Display(Name = "Advanced Periodontitis")]
        public bool IsAdvPerio { get; set; }
        [Display(Name = "Class")]
        public string? ClassType { get; set; }
        [Display(Name = "Overjet")]
        public bool IsOverjet { get; set; }
        [Display(Name = "Overbite")]
        public bool IsOverbite { get; set; }
        [Display(Name = "Midline Deviation")]
        public bool IsMidlineDeviation { get; set; }
        [Display(Name = "Crossbite")]
        public bool IsCrossbite { get; set; }
        [Display(Name = "Orthodontic Application")]
        public string? OrthoApplication { get; set; }
        [Display(Name = "Clenching")]
        public bool IsClenching { get; set; }
        [Display(Name = "Clicking")]
        public bool IsClicking { get; set; }
        [Display(Name = "Trismus")]
        public bool IsTrismus { get; set; }
        [Display(Name = "Muscle Pain")]
        public bool IsMusclePain { get; set; }
    }
}
