using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Domain.Repositories;

/// <summary>
/// Repository interface for managing game rooms
/// </summary>
public interface IGameRoomRepository
{
    /// <summary>
    /// Creates a new game room.
    /// </summary>
    /// <param name="game">The game room to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateAsync(Game game);

    /// <summary>
    /// Gets a game room by its ID.
    /// </summary>
    /// <param name="id">The ID of the game room to retrieve.</param>
    /// <returns>The game room if found, null otherwise.</returns>
    Task<Game?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all available game rooms.
    /// </summary>
    /// <returns>A list of all game rooms.</returns>
    Task<IEnumerable<Game>> GetAllAsync();

    /// <summary>
    /// Updates an existing game room.
    /// </summary>
    /// <param name="game">The game room with updated information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Game game);

    /// <summary>
    /// Deletes a game room.
    /// </summary>
    /// <param name="id">The ID of the game room to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id);
}