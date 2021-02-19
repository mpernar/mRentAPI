using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIForRentersAPI.Models
{
    public partial class EmailTemplate
    {
        [Key]
        [Column("EmailTemplateID")]
        public int EmailTemplateId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        public string TemplateContent { get; set; }
    }
}
