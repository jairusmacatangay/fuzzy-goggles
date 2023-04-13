using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class Tooth
    {
        [Key]
        public int Id { get; set; }

        public int DentalChartId { get; set; }

        [ForeignKey("DentalChartId")]
        [ValidateNever]
        public DentalChart? DentalChart { get; set; }

        public int ToothNo { get; set; }

        public string? Type { get; set; }

        public int Condition { get; set; }

        public int BuccalOrLabial { get; set; }

        public int Lingual { get; set; }

        public int Occlusal { get; set; }

        public int Mesial { get; set; }

        public int Distal { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
    }
}
