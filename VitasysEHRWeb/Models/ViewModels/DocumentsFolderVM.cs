using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.ViewModels
{
    public class DocumentsFolderVM
    {
        public int? Id { get; set; }
        public Folder? Folder { get; set; }
        public IEnumerable<Folder>? FolderList { get; set; }
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public Document? Document { get; set; }
        public Clinic? Clinic { get; set; }
        public int? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public IEnumerable<Document>? DocumentList { get; set; }
        public IEnumerable<SelectListItem>? FolderTypeList{ get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public DateTime? DateAdded { get; set; }
        public DateTime? LastModified { get; set; }

    }
}
