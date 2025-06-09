using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public class Player
{
    public Guid GameId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public ICollection<CardType> PlayedCards { get; set; } = [];
    public CardType? HoldingCard { get; set; }
    public int Score { get; set; } = 0;
    public bool Protected { get; set; } = false;
}
