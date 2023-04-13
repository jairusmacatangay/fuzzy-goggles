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
    public class MedicalHistory
    {
        [Key]
        public int Id { get; set; }

        public int AllergyId { get; set; }

        [ForeignKey("AllergyId")]
        [ValidateNever]
        public Allergy? Allergy { get; set; }

        public int ROSId { get; set; }

        [ForeignKey("ROSId")]
        [ValidateNever]
        public ReviewOfSystem? ReviewOfSystem { get; set; }

        public bool IsGoodHealth { get; set; }

        public bool IsMedTreatment { get; set; }

        public string? IsConditionTreated { get; set; }

        public bool IsIllnessOperation { get; set; }

        public string? IllnessOperation { get; set; }

        public bool IsHospitalized { get; set; }

        public string? Hospitalized { get; set; }

        public bool IsMedication { get; set; }

        public string? Medication { get; set; }

        public bool IsTobacco { get; set; }

        public bool IsDrugs { get; set; }


        [Display(Name = "Bleeding Time")]
        public string? BleedingTime { get; set; }

        [Display(Name = "Blood Type")]
        [Required]
        public string? BloodType { get; set; }

        [Display(Name = "Blood Pressure")]
        [Required]
        public string? BloodPressure { get; set; }

        public bool IsPregnant { get; set; }

        public bool IsNursing { get; set; }

        public bool IsBirthControl { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public DateTime? LastModified { get; set; }

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        [ValidateNever]
        public Patient? Patient { get; set; }

        public bool IsEditable { get; set; }
    }
}
