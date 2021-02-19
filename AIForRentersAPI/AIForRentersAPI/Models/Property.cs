using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIForRentersAPI.Models
{
    public partial class Property
    {
        public Property()
        {
            Unit = new HashSet<Unit>();
        }

        [Key]
        [Column("PropertyID")]
        public int PropertyId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Location { get; set; }

        [InverseProperty("Property")]
        public virtual ICollection<Unit> Unit { get; set; }
    }
}
