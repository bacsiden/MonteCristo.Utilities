using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MonteCristo.MongoDB.Framework
{
    public interface IBaseRepository<T> where T : class
    {
        T Get(Guid id);
        Task<T> GetAsync(Guid id);

        Task<T> GetAsync(Expression<Func<T, bool>> predicate);

        IMongoQueryable<T> Find(Expression<Func<T, bool>> predicate);

        Task<T> AddAsync(T model);

        Task AddRangeAsync(IEnumerable<T> list);

        Task<T> UpsertAsync(T model);

        Task<T> UpdateAsync(T model);

        Task DeleteAsync(Guid id);
        Task DeleteAsync(string id);
        Task DeleteManyAsync(string fieldName, object value);
        Task<int> BuildCodeAsync(Guid clinicId, string fieldName, string type = null);

        Task SetAsync(Guid id, string fieldName, dynamic value);
        Task SetAsync(string id, string fieldName, dynamic value);

        IFindFluent<T, T> Find(IEnumerable<Guid> Ids);
    }
}