using MongoDB.Bson;
using MongoDB.Driver;
using MonteCristo.Application.Models.Filter;
using MonteCristo.Application.Models.Framework;
using MonteCristo.Application.Repositories;
using MonteCristo.MongoDB.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonteCristo.Database.Repositories
{
    public class UserRepository : BaseRepository<ApplicationUser>, IUserRepository
    {
        private readonly IMongoDatabase _db;
        public UserRepository(IMongoDatabase db) : base(db, "Users")
        {
            _db = db;
        }

        public Task<List<ApplicationUser>> FindAsync(UserFilter userFilter)
        {
            var builder = Builders<ApplicationUser>.Filter;
            var filter = builder.Regex(m => m.Name, new BsonRegularExpression($"{userFilter.query}", "i")) | 
                builder.Regex(m => m.Email, new BsonRegularExpression($"{userFilter.query}", "i"));

            return _collection.Find(filter).Limit(userFilter.limit).ToListAsync();
        }

        public List<ApplicationUser> Get()
        {
            return _collection.AsQueryable().ToList();
        }

        public Task<ApplicationUser> GetById(string id)
        {
            return _collection.Find(Builders<ApplicationUser>.Filter.Eq(m => m.Id, id)).FirstOrDefaultAsync();
        }
    }
}
