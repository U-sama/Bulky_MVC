using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext db) : base(db) 
        {
            
        }
        public void Update(Product obj)
        {
            Product product =  _db.Products.FirstOrDefault(a => a.Id == obj.Id);
            if(product != null)
            {
                product.Title = obj.Title;
                product.ISBN = obj.ISBN;
                product.Price = obj.Price;
                product.Price50 = obj.Price50;
                product.Price100 = obj.Price100;
                product.Description = obj.Description;
                product.CategoryId = obj.CategoryId;
                product.Author = obj.Author;
                if(obj.ImageUrl != null)
                {
                    product.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
