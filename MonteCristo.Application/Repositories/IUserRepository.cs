using MonteCristo.Application.Models.Filter;
using MonteCristo.Application.Models.Framework;
using MonteCristo.MongoDB.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonteCristo.Application.Repositories
{
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
        List<ApplicationUser> Get();

        Task<ApplicationUser> GetById(string id);
        Task<List<ApplicationUser>> FindAsync(UserFilter userFilter);
    }
}
