using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CRMTicketingSystem.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Problem Title")]
        public string Subject { get; set; }
        [Required]
        [Display(Name = "Problem Description")]
        public string  Description { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string TicketStatus { get; set; }
        public string Status { get; set; }
        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public DateTime CreatedDate { get; set; }

        public string Review { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}
