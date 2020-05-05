using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository
{
    public class UnitOfWork :IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            CoverType = new CoverTypeRepository(_db);
            Company = new CompanyRepository(_db);
            Product = new ProductRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            SP_call = new SP_call(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetails = new OrderDetailsRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            Ticket = new TicketRepository(_db);
            Help = new HelpRepository(_db);
        }

        public ICategoryRepository Category { get; private set; }
        public ICoverTypeRepository CoverType { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public ISP_call SP_call { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public ITicketRepository Ticket { get; private set; }
        public IHelpRepository Help { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
