using Microsoft.EntityFrameworkCore;
using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Email);
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => new { e.GameId, e.UserEmail });
                
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserEmail)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Game>()
                    .WithMany(g => g.Players)
                    .HasForeignKey(p => p.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                
                // Configure the relationship with Players
                entity.HasMany(e => e.Players)
                    .WithOne()
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure the relationship with Host User
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.HostEmail)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
