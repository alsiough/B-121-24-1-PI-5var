using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using JobPortal.DAL.Context;
using JobPortal.DAL.Interfaces;

namespace JobPortal.DAL.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly JobPortalContext _db;
        private readonly DbSet<T> _dbSet;

        public Repository(JobPortalContext context)
        {
            _db = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll() => _dbSet.ToList();

        public T Get(int id) => _dbSet.Find(id);

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate).ToList();

        public IEnumerable<T> FindWithIncludes(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query.Where(predicate).ToList();
        }

        public void Create(T item) => _dbSet.Add(item);

        public void Update(T item)
        {
            _db.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            T item = _dbSet.Find(id);
            if (item != null) _dbSet.Remove(item);
        }
    }
}