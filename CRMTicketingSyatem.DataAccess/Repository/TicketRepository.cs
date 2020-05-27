using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        private readonly ApplicationDbContext _db;

        public TicketRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Ticket ticket)
        {
            var objFromDb = _db.Tickets.FirstOrDefault(s => s.Id == ticket.Id);
            if (objFromDb != null)
            {
                objFromDb.Review = ticket.Review;
                objFromDb.ReviewDate = DateTime.Now;
                objFromDb.TicketStatus = "4";
                objFromDb.Status = "Reviewed";
            }
        }
    }
}
