using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class PlayerUpdateDto
{
    public string UserEmail { get; set; } = string.Empty;
    public bool IsProtected { get; set; } = false;
    public List<CardType> HoldingCards { get; set; } = [];
    public ICollection<CardType> PlayedCards { get; set; } = [];
    public int Score { get; set; } = 0;
    
    public PlayerUpdateDto() { }

    public PlayerUpdateDto(Player player)
    {
        UserEmail = player.UserEmail;
        IsProtected = player.Protected;
        HoldingCards = player.HoldingCards.ToList();
        PlayedCards = player.PlayedCards.ToList();
        Score = player.Score;
    }
}
