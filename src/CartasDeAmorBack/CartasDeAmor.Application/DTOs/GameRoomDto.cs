using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Application.DTOs;

public class GameRoomDto(Game game)
{
    public Guid Id { get; set; } = game.Id;
    public string RoomName { get; set; } = game.Name;
    public string OwnerEmail { get; set; } = game.HostEmail;
    public bool HasPassword { get; set; } = !string.IsNullOrEmpty(game.Password);
    public int CurrentPlayers { get; set; } = game.Players.Count;
    public DateTime CreatedAt { get; set; } = game.CreatedAt;
}