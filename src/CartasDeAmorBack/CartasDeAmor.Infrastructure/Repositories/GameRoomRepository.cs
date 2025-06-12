using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CartasDeAmor.Infrastructure.Repositories;

public class GameRoomRepository : IGameRoomRepository
{
    private readonly AppDbContext _context;

    public GameRoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Game game)
    {
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _context.Games
            .Include(g => g.Players.OrderBy(p => p.Id))
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _context.Games
            .Include(g => g.Players.OrderBy(p => p.Id))
            .ToListAsync();
    }

    public async Task UpdateAsync(Game game)
    {
        game.UpdatedAt = DateTime.UtcNow;
        _context.Games.Update(game);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game != null)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }
    }
}
