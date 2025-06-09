using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CartasDeAmor.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly AppDbContext _context;

        public PlayerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetByGameAndUserAsync(Guid gameId, string userEmail)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.GameId == gameId && p.UserEmail == userEmail);
        }

        public async Task<IEnumerable<Player>> GetByGameAsync(Guid gameId)
        {
            return await _context.Players
                .Where(p => p.GameId == gameId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Player>> GetByUserAsync(string userEmail)
        {
            return await _context.Players
                .Where(p => p.UserEmail == userEmail)
                .ToListAsync();
        }

        public async Task AddAsync(Player player)
        {
            await _context.Players.AddAsync(player);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Player player)
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid gameId, string userEmail)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.GameId == gameId && p.UserEmail == userEmail);
            
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }
        }
    }
}
