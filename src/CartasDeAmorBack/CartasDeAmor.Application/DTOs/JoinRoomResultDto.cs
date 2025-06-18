using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Application.DTOs;

public class JoinRoomResultDto
{
    public string RoomId { get; set; } = string.Empty;
    public int PlayerId { get; set; }
    public string HostEmail { get; set; } = string.Empty;
    public ICollection<string> Players { get; set; } = [];

    public JoinRoomResultDto(Game game, Player player)
    {
        RoomId = game.Id.ToString();
        PlayerId = player.Id;
        HostEmail = game.HostEmail;
        Players = game.Players.Select(p => p.UserEmail).ToList();
    }
}