using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Domain.Repositories
{
    public interface IPlayerRepository
    {
        Task<Player?> GetByGameAndUserAsync(Guid gameId, string userEmail);
        Task<IEnumerable<Player>> GetByGameAsync(Guid gameId);
        Task<IEnumerable<Player>> GetByUserAsync(string userEmail);
        Task AddAsync(Player player);
        Task UpdateAsync(Player player);
        Task DeleteAsync(Guid gameId, string userEmail);
    }
}
