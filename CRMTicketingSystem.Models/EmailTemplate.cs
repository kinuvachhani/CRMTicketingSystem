using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CRMTicketingSystem.Models
{
    public class EmailTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string TemplateName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
