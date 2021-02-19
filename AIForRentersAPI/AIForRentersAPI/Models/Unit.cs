using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIForRentersAPI.Models
{
    public partial class Unit
    {
        public Unit()
        {
            Availability = new HashSet<Availability>();
        }

        [Key]
        [Column("UnitID")]
        public int UnitId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public int Capacity { get; set; }
        public double Price { get; set; }
        [Column("PropertyID")]
        public int PropertyId { get; set; }

        [ForeignKey(nameof(PropertyId))]
        [InverseProperty("Unit")]
        public virtual Property Property { get; set; }
        [InverseProperty("Unit")]
        public virtual ICollection<Availability> Availability { get; set; }
    }
}
