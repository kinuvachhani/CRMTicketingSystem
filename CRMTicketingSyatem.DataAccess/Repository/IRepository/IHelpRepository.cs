using CRMTicketingSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository.IRepository
{
    public interface IHelpRepository : IRepository<Help>
    {
        void Update(Help help);
    }
}
