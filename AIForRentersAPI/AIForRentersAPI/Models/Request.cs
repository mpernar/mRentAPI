using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIForRentersAPI.Models
{
    public partial class Request
    {
        [Key]
        [Column("RequestID")]
        public int RequestId { get; set; }
        [Required]
        [StringLength(50)]
        public string Property { get; set; }
        [Required]
        [StringLength(50)]
        public string Unit { get; set; }
        [Column(TypeName = "date")]
        public DateTime FromDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime ToDate { get; set; }
        public double PriceUponRequest { get; set; }
        public bool Confirmed { get; set; }
        public bool Processed { get; set; }
        public bool Sent { get; set; }
        public int NumberOfPeople { get; set; }
        [Required]
        [StringLength(50)]
        public string ResponseSubject { get; set; }
        [Required]
        public string ResponseBody { get; set; }
        [Column("ClientID")]
        public int ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        [InverseProperty("Request")]
        public virtual Client Client { get; set; }

        public void UpdateRequest(Request request, string content, string subject)
        {
            using (var context = new AIForRentersDbContext())
            {
                context.Request.Attach(request);

                request.Processed = true;
                request.Confirmed = false;
                request.ResponseBody = content;
                request.ResponseSubject = subject;

                context.SaveChanges();
            }
        }
    }
}
