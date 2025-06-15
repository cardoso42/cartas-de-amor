using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class PlayerStatusDto
{
    public string UserEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public PlayerStatus Status { get; set; } = PlayerStatus.Active;
    public bool IsProtected { get; set; } = false;
    public int Score { get; set; } = 0;
    public int CardsInHand { get; set; } = 1;

    public PlayerStatusDto() { }

    public PlayerStatusDto(Player player)
    {
        UserEmail = player.UserEmail;
        Username = player.Username;
        IsProtected = player.IsProtected();
        Score = player.Score;
        CardsInHand = player.HoldingCards.Count;
        Status = PlayerStatus.Active;
    }
}
