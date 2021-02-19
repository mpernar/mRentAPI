using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIForRentersAPI.Models
{
    public partial class Availability
    {
        [Key]
        [Column("AvailabilityID")]
        public int AvailabilityId { get; set; }
        [Column(TypeName = "date")]
        public DateTime FromDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime ToDate { get; set; }
        public bool Available { get; set; }
        [Column("UnitID")]
        public int UnitId { get; set; }

        [ForeignKey(nameof(UnitId))]
        [InverseProperty("Availability")]
        public virtual Unit Unit { get; set; }

        public void AddAvailability(Unit unit, DateTime fromDate, DateTime toDate)
        {
            using (var context = new AIForRentersDbContext())
            {
                context.Unit.Attach(unit);
                Availability newAvailability = new Availability
                {
                    Unit = unit,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Available = false
                };

                context.Availability.Add(newAvailability);
                context.SaveChanges();
            }
        }
    }
}
