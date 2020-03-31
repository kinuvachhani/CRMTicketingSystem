using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CRMTicketingSystem.Models
{
    public class ApplicationUser :IdentityUser
    {
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
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [NotMapped]
        public string Role { get; set; }
    }

    public enum RoleList
    {
        Admin,
        CompanyCustomer,
        Employee,
        IndividualUser
    }
}
