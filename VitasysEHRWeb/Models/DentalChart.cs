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
    public class DentalChart
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        [ValidateNever]
        public Patient? Patient { get; set; }

        public int ClinidId { get; set; }
        [ForeignKey("ClinidId")]
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        public int OralCavityId { get; set; }
        [ForeignKey("OralCavityId")]
        [ValidateNever]
        public OralCavity? OralCavity { get; set; }

        public string? Status { get; set; }
        public bool IsArchived { get; set; }

        public DateTime? EncounterDate { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
