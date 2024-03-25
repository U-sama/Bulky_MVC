using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        internal DbSet<T> dbset;
        public Repository( ApplicationDbContext db)
        {
            _db = db;
            dbset = _db.Set<T>();
            // _db.Categories == dbset
        }
        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? IncludeProperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
                query = dbset;
            else
                query = dbset.AsNoTracking();

            if (!string.IsNullOrEmpty(IncludeProperties))
            {
                foreach (var item in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            query = query.Where(filter);
            return query.FirstOrDefault();
        }
        //IncludeProperties: "Category, Product"
        public IEnumerable<T> GetAll(string? IncludeProperties = null)
        {
            IQueryable<T> query = dbset;
            if (!string.IsNullOrEmpty(IncludeProperties))
            {
                foreach (var item in IncludeProperties.Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbset.RemoveRange(entities);
        }
    }
}
