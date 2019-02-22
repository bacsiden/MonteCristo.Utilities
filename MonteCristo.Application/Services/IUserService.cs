using MonteCristo.Application.Models.Filter;
using MonteCristo.Application.Models.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MonteCristo.Application.Services
{
    public interface IUserService
    {
        Task AddAsync(ApplicationUser user);

        Task<List<ApplicationUser>> Find(Expression<Func<ApplicationUser, bool>> predicate);

        Task<ApplicationUser> GetAsync(string id);

        Task<ApplicationUser> GetByUsernameAsync(string username);

        Task<ApplicationUser> UpdateAsync(ApplicationUser user);

        Task SetAsync(string id, string fieldName, dynamic value);

        Task DeleteAsync(string id);

        Task<List<ApplicationUser>> FindAsync(UserFilter userFilter);
    }
}
