using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class PrivatePlayerUpdateDto
{
    public string UserEmail { get; set; } = string.Empty;
    public PlayerStatus Status { get; set; } = PlayerStatus.Active;
    public List<CardType> HoldingCards { get; set; } = [];
    public List<CardType> PlayedCards { get; set; } = [];
    public int Score { get; set; } = 0;
    
    public PrivatePlayerUpdateDto() { }

    public PrivatePlayerUpdateDto(Player player)
    {
        UserEmail = player.UserEmail;
        Status = player.Status;
        HoldingCards = player.HoldingCards.ToList();
        PlayedCards = player.PlayedCards.ToList();
        Score = player.Score;
    }
}
