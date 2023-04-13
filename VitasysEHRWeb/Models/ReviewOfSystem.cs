using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models
{
    public class ReviewOfSystem
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "High Blood Pressure")]
        public bool IsHbp { get; set; }
        [Display(Name = "Low Blood Pressure")]
        public bool IsLbp { get; set; }
        [Display(Name = "Epilepsy / Convulsions")]
        public bool IsEpilepsy { get; set; }
        [Display(Name = "AIDs or HIV Infection")]
        public bool IsAids { get; set; }
        [Display(Name = "Sexually Transmitted Disease")]
        public bool IsStd { get; set; }
        [Display(Name = "Stomach Troubles / Ulcers")]
        public bool IsStomach { get; set; }
        [Display(Name = "Fainting / Seizure")]
        public bool IsFainting { get; set; }
        [Display(Name = "Rapid Weight Loss")]
        public bool IsWeightLoss { get; set; }
        [Display(Name = "Radiation Therapy")]
        public bool IsRadiation { get; set; }
        [Display(Name = "Joint Replacement / Implant")]
        public bool IsJointReplacement { get; set; }
        [Display(Name = "Heart Surgery")]
        public bool IsHeartSurgery { get; set; }
        [Display(Name = "Heart Attack")]
        public bool IsHeartAttack { get; set; }
        [Display(Name = "Thyroid Problem")]
        public bool IsThyroid { get; set; }
        [Display(Name = "Heart Disease")]
        public bool IsHeartDisease { get; set; }
        [Display(Name = "Heart Murmur")]
        public bool IsHeartMurmur { get; set; }
        [Display(Name = "Hepatitis / Liver Disease")]
        public bool IsHepa { get; set; }
        [Display(Name = "Rheumatic Fever")]
        public bool IsRheumatic { get; set; }
        [Display(Name = "Hay Fever / Allergies")]
        public bool IsHayFever { get; set; }
        [Display(Name = "Respiratory Problems")]
        public bool IsRespiratory { get; set; }
        [Display(Name = "Hepatitis / Jaundice")]
        public bool IsJaundice { get; set; }
        [Display(Name = "Tuberculosis")]
        public bool IsTuberculosis { get; set; }
        [Display(Name = "Swollen Ankles")]
        public bool IsSwollenAnkles { get; set; }
        [Display(Name = "Kidney Disease")]
        public bool IsKidney { get; set; }
        [Display(Name = "Diabetes")]
        public bool IsDiabetes { get; set; }
        [Display(Name = "Chest Pain")]
        public bool IsChestPain { get; set; }
        [Display(Name = "Stroke")]
        public bool IsStroke { get; set; }
        [Display(Name = "Cancer / Tumors")]
        public bool IsCancer { get; set; }
        [Display(Name = "Anemia")]
        public bool IsAnemia { get; set; }
        [Display(Name = "Angina")]
        public bool IsAngina { get; set; }
        [Display(Name = "Asthma")]
        public bool IsAsthma { get; set; }
        [Display(Name = "Emphysema")]
        public bool IsEmphysema { get; set; }
        [Display(Name = "Bleeding Problems")]
        public bool IsBleeding { get; set; }
        [Display(Name = "Blood Diseases")]
        public bool IsBloodDisease { get; set; }
        [Display(Name = "Head Injury")]
        public bool IsHeadInjury { get; set; }
        [Display(Name = "Arthritis / Rheumatism")]
        public bool IsArthritis { get; set; }
        public string? Other { get; set; }
    }
}
