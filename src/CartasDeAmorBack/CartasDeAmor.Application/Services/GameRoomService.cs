using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Domain.Services;

public class GameRoomService : IGameRoomService
{
    private readonly IGameRoomRepository _roomRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IUserRepository _userRepository;

    public GameRoomService(IGameRoomRepository roomRepository, IPlayerRepository playerRepository, IUserRepository userRepository)
    {
        _roomRepository = roomRepository;
        _playerRepository = playerRepository;
        _userRepository = userRepository;
    }

    private async Task<Player> CreatePlayer(Game game, string userEmail)
    {
        var user = await _userRepository.GetByEmailAsync(userEmail) ?? throw new InvalidOperationException("User not found");

        var currentId = game.Players.LastOrDefault()?.Id ?? 0;

        return new Player
        {
            GameId = game.Id,
            Username = user.Username,
            UserEmail = userEmail,
            Id = currentId + 1,
            HoldingCards = []
        };
    }

    public async Task<Guid> CreateRoomAsync(string name, string creatorEmail, string? password = null)
    {
        var game = new Game
        {
            Name = name,
            Password = password,
            HostEmail = creatorEmail,
        };

        var host = await CreatePlayer(game, creatorEmail);
        game.Players.Add(host);

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
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (game.Players.Any(p => p.UserEmail == userEmail))
        {
            throw new InvalidOperationException("User is already in the room");
        }

        if (game.Players.Count >= 4)
        {
            throw new InvalidOperationException("Room is full");
        }

        var player = await CreatePlayer(game, userEmail);
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
