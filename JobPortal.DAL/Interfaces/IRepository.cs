using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JobPortal.DAL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        IEnumerable<T> FindWithIncludes(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        void Create(T item);
        void Update(T item);
        void Delete(int id);
    }
}