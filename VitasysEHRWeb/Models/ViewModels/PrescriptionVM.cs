using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class PrescriptionVM
    {
        public int? Id { get; set; }
        public Prescription? Prescription { get; set; }
        public Clinic? Clinic { get; set; }
        public Patient Patient { get; set; }
        public IEnumerable<Prescription> Prescriptions { get; set; }
        public string? ClinicName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? ZipCode { get; set; }
        public string? OfficePhone { get; set; }
        public string? MobilePhone { get; set; }
        public string? Drug { get; set; }
        public string? Dosage { get; set; }
        public string? Dose { get; set; }
        public string? EmailAddress { get; set; }
        public int? Quantity { get; set; }
        public string? DateAdded { get; set; }
        public string? Fullname { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DOB { get; set; }
        public string? PatientAddress { get; set; }
        public string? PatientCity { get; set; }
        public string? PatientProvince { get; set; }
        public string? PatientZipCode { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Sig { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
