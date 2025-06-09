using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Domain.Services;

public class GameRoomService : IGameRoomService
{
    private readonly IGameRoomRepository _roomRepository;
    private readonly IPlayerRepository _playerRepository;

    public GameRoomService(IGameRoomRepository roomRepository, IPlayerRepository playerRepository)
    {
        _roomRepository = roomRepository;
        _playerRepository = playerRepository;
    }

    public async Task<Guid> CreateRoomAsync(string name, string creatorEmail, string? password = null)
    {
        var host = new Player { UserEmail = creatorEmail };
        var game = new Game
        {
            Name = name,
            Password = password,
            Players = [host],
            HostEmail = creatorEmail,
        };

        await _roomRepository.CreateAsync(game);
        return game.Id;
    }

    public async Task DeleteRoomAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        if (game.HostEmail != userEmail)
        {
            throw new InvalidOperationException("Only the host can delete the room");
        }

        await _roomRepository.DeleteAsync(roomId);
    }

    public async Task AddUserToRoomAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        if (game.Players.Any(p => p.UserEmail == userEmail))
        {
            throw new InvalidOperationException("User is already in the room");
        }

        if (game.Players.Count >= 4)
        {
            throw new InvalidOperationException("Room is full");
        }

        var player = new Player
        {
            GameId = roomId,
            UserEmail = userEmail
        };

        game.Players.Add(player);
        await _roomRepository.UpdateAsync(game);
    }

    public async Task RemoveUserFromRoomAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        var player = game.Players.FirstOrDefault(p => p.UserEmail == userEmail);
        if (player == null)
        {
            throw new InvalidOperationException("User is not in the room");
        }

        game.Players.Remove(player);
        await _roomRepository.UpdateAsync(game);
        await _playerRepository.DeleteAsync(roomId, userEmail);
    }
}
