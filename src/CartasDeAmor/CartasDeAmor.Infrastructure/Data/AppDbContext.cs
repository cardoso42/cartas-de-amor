using Microsoft.EntityFrameworkCore;
using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
