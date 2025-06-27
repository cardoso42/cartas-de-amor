using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByEmailsAsync(IEnumerable<string> emails);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(string email);
    }
}
