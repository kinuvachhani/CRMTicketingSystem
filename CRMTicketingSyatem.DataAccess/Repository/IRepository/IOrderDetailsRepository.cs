using CRMTicketingSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository.IRepository
{
    public interface IOrderDetailsRepository : IRepository<OrderDetails>
    {
        void Update(OrderDetails obj);
    }
}
