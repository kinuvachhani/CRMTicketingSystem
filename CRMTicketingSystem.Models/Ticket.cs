using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CRMTicketingSystem.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        public string Subject { get; set; }
        public string  Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string Review { get; set; }
    }
}
