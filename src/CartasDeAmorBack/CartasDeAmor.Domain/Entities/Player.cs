using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public class Player
{
    public int Id { get; set; }
    public Guid GameId { get; set; }
    public required string Username { get; set; }
    public required string UserEmail { get; set; }
    public ICollection<CardType> PlayedCards { get; set; } = [];
    public required IList<CardType> HoldingCards { get; set; }
    public int Score { get; set; } = 0;
    public bool Protected { get; set; } = false;
}
