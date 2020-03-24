using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRMTicketingSystem.Models.ViewModels
{
    public class TicketVM
    {
        public Ticket Ticket { get; set; }
        public IEnumerable<SelectListItem> ProductList { get; set; }
    }
}
