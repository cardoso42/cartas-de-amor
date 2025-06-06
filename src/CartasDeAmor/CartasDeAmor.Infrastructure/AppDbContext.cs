using Microsoft.EntityFrameworkCore;
using CartasDeAmor.Domain.Models;

namespace CartasDeAmor.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
