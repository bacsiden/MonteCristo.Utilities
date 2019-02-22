using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MonteCristo.MongoDB.Framework
{
    public abstract class BaseRepository<T> where T : class
    {
        protected IMongoCollection<T> _collection;

        public BaseRepository(IMongoDatabase database, string collectionName = null) => _collection = database.GetCollection<T>(string.IsNullOrEmpty(collectionName) ? $"{typeof(T).Name}s" : collectionName);

        protected FilterDefinition<T> GetFilter(Guid clinicId) => Builders<T>.Filter.Eq("ClinicId", clinicId);

        public virtual T Get(Guid id) => _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefault();

        public virtual Task<T> GetAsync(Guid id) => _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();

        public virtual Task<T> GetAsync(Expression<Func<T, bool>> predicate) => _collection.AsQueryable().FirstOrDefaultAsync(predicate);

        public virtual IMongoQueryable<T> Find(Expression<Func<T, bool>> predicate) => _collection.AsQueryable().Where(predicate);

        public virtual IFindFluent<T, T> Find(IEnumerable<Guid> Ids)
        {
            var query = Builders<T>.Filter.In("_id", Ids);
            return _collection.Find(query);
        }

        public virtual async Task<T> AddAsync(T model)
        {
            await _collection.InsertOneAsync(model);
            return model;
        }

        public virtual Task AddRangeAsync(IEnumerable<T> list) => _collection.InsertManyAsync(list);

        public virtual Task<T> UpsertAsync(T model)
        {
            if ((Guid)typeof(T).GetProperty("Id").GetValue(model) == Guid.Empty)
                return AddAsync(model);
            else
                return UpdateAsync(model);
        }

        public virtual async Task<T> UpdateAsync(T model)
        {
            var id = typeof(T).GetProperty("Id").GetValue(model);
            var filter_id = Builders<T>.Filter.Eq("_id", id);
            var result = await _collection.ReplaceOneAsync(filter_id, model);

            return model;
        }

        public virtual Task SetAsync(string id, string fieldName, dynamic value)
        {
            var filter_id = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            return _collection.UpdateOneAsync(filter_id, Builders<T>.Update.Set(fieldName, value));
        }

        public virtual Task SetAsync(Guid id, string fieldName, dynamic value)
        {
            var filter_id = Builders<T>.Filter.Eq("_id", id);
            return _collection.UpdateOneAsync(filter_id, Builders<T>.Update.Set(fieldName, value));
        }

        public virtual Task DeleteAsync(Guid id) => _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));

        public virtual Task DeleteAsync(string id) => _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", ObjectId.Parse(id)));

        public virtual Task DeleteManyAsync(string fieldName, object value)
        {
            var filter  = Builders<T>.Filter.Eq(fieldName, value);
            return _collection.DeleteManyAsync(filter);
        }

        public async Task<int> BuildCodeAsync(Guid clinicId, string fieldName, string type = null)
        {
            var sorter = Builders<T>.Sort.Descending(fieldName);
            var filter = Builders<T>.Filter.Eq("ClinicId", clinicId);
            if (!string.IsNullOrEmpty(type))
                filter &= Builders<T>.Filter.Eq("_t", type);

            var result = _collection.Find(filter).Sort(sorter).Limit(1);
            if (result.CountDocuments() == 0)
            {
                return 1;
            }
            var item = await result.FirstOrDefaultAsync();
            return (int)typeof(T).GetProperty(fieldName).GetValue(item) + 1;
        }
    }
}