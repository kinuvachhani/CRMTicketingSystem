using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CRMTicketingSystem.Models
{
    public class Help
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Problem Title")]
        public string Subject { get; set; }
        [Required]
        [Display(Name = "Problem Description")]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public int TicketStatus { get; set; }
        public DateTime CreatedDate { get; set; }

        public string Review { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}
