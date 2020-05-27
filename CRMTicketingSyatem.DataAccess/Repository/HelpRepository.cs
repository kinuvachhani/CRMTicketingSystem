using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository
{
    public class HelpRepository : Repository<Help>, IHelpRepository
    {
        private readonly ApplicationDbContext _db;

        public HelpRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Help help)
        {
            var objFromDb = _db.Helps.FirstOrDefault(s => s.Id == help.Id);
            if (objFromDb != null)
            {
                objFromDb.Review = help.Review;
                objFromDb.ReviewDate = DateTime.Now;
                objFromDb.TicketStatus = "4";

            }
        }
    }
}
