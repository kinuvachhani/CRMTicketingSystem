using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository
{
    public class CategoryRepository :Repository<Category> ,ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) :base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            var ObjFromDb = _db.Categories.FirstOrDefault(s => s.Id == category.Id);
            if (ObjFromDb != null)
            {
                ObjFromDb.Name = category.Name;
            }
            _db.SaveChanges();
        }
    }
}
