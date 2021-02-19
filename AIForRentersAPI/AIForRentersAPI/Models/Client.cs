using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIForRentersAPI.Models
{
    public partial class Client
    {
        public Client()
        {
            Request = new HashSet<Request>();
        }

        [Key]
        [Column("ClientID")]
        public int ClientId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Surname { get; set; }
        [Required]
        [StringLength(50)]
        public string Email { get; set; }

        [InverseProperty("Client")]
        public virtual ICollection<Request> Request { get; set; }
    }
}
