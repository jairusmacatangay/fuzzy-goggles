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
    public class Document
    {
        [Key]
        public int Id { get; set; }

        public int FolderId { get; set; }
        [ForeignKey("FolderId")]
        [ValidateNever]
        public Folder? Folder { get; set; }
        [Required]
        [RegularExpression(@"^[\w\- ]+$", ErrorMessage = "Document name should only contain alphanumeric numbers, dash or underscore")]
        public string Name { get; set; }

        [Required, Display(Name = "Upload File")]
        public string? DocumentUrl { get; set; }

        public bool IsArchived { get; set; }
        public bool IsShared { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
    }
}
