using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CRMTicketingSystem.Models
{
    public class Company
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        [MaxLength(50)]
        public string City { get; set; }
        [Required]
        [MaxLength(50)]
        public string State { get; set; }
        [Required]
        [MaxLength(50)]
        public string PostalCode { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        public bool IsAuthorizedCompany { get; set; }
    }
}
