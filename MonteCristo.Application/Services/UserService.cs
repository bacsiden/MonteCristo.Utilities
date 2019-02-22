using MonteCristo.Application.Models;
using MonteCristo.Application.Models.Filter;
using MonteCristo.Application.Models.Framework;
using MonteCristo.Application.Repositories;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MonteCristo.Application.Services
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task AddAsync(ApplicationUser user) => _userRepository.AddAsync(user);

        public Task DeleteAsync(string id) => _userRepository.DeleteAsync(id);

        public Task<List<ApplicationUser>> Find(Expression<Func<ApplicationUser, bool>> predicate) => _userRepository.Find(predicate).ToListAsync();

        public Task<List<ApplicationUser>> FindAsync(UserFilter userFilter) => _userRepository.FindAsync(userFilter);

        public Task<ApplicationUser> GetAsync(string id) => _userRepository.GetAsync(m => m.Id == id);

        public Task<ApplicationUser> GetByUsernameAsync(string username)
        {
            return _userRepository.GetAsync(m => m.UserName == username);
        }

        public Task SetAsync(string id, string fieldName, dynamic value) => _userRepository.SetAsync(id, fieldName, value);

        public Task<ApplicationUser> UpdateAsync(ApplicationUser user) => _userRepository.UpdateAsync(user);
    }
}
