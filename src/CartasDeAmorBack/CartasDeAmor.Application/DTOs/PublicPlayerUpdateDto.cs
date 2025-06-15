using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class PublicPlayerUpdateDto
{
    public string UserEmail { get; set; } = string.Empty;
    public PlayerStatus Status { get; set; } = PlayerStatus.Active;
    public List<CardType> PlayedCards { get; set; } = [];
    public int HoldingCardsCount { get; set; } = 0;
    public int Score { get; set; } = 0;
    
    public PublicPlayerUpdateDto() { }

    public PublicPlayerUpdateDto(Player player)
    {
        UserEmail = player.UserEmail;
        Status = player.Status;
        HoldingCardsCount = player.HoldingCards.Count;
        PlayedCards = player.PlayedCards.ToList();
        Score = player.Score;
    }
}
