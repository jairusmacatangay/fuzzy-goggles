using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class InvoiceVM
    {
        public Invoice? Invoice { get; set; }
        public string? UserName { get; set; }
        public string? PatientName { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ClinicCity { get; set; }
        public string? ClinicProvince { get; set; }
        public string? ClinicZipCode { get; set; }
        public string? ClinicOfficePhone { get; set; }
        public string? ClinicMobilePhone { get; set; }
        public string? ClinicEmailAddress { get; set; }
        public List<TreatmentRecord>? TreatmentRecordList { get; set; }
    }
}
