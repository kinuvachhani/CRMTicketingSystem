using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var ObjFromDb = _db.Products.FirstOrDefault(s => s.Id == product.Id);
            if (ObjFromDb != null)
            {
                if(product.ImageUrl != null)
                {
                    ObjFromDb.ImageUrl = product.ImageUrl;
                }
                if (product.PreviewUrl != null)
                {
                    ObjFromDb.PreviewUrl = product.PreviewUrl;
                }
                ObjFromDb.ISBN= product.ISBN;
                ObjFromDb.Price= product.Price;
                ObjFromDb.Price50= product.Price50;
                ObjFromDb.ListPrice= product.ListPrice;
                ObjFromDb.Price100= product.Price100;
                ObjFromDb.Title= product.Title;
                ObjFromDb.Discription= product.Discription;
                ObjFromDb.CategoryId= product.CategoryId;
                ObjFromDb.Author= product.Author;
                ObjFromDb.CoverTypeId= product.CoverTypeId;
            }
        }
    }
}
