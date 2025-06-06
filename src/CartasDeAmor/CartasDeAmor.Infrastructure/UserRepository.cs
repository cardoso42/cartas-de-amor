using System.Linq;
using CartasDeAmor.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CartasDeAmor.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        public void AddUser(string username, string hashedPassword)
        {
            var user = new User { Username = username, HashedPassword = hashedPassword };
            _context.Users.Add(user);
            _context.SaveChanges();
        }
    }
}
