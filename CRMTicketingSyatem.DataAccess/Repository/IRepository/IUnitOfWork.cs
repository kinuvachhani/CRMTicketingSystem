using System;
using System.Collections.Generic;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Category { get; }

        ISP_call SP_call { get; }

        void Save();
    }
}
