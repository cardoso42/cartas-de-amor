using CartasDeAmor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CartasDeAmor.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(string email);
    }
}
