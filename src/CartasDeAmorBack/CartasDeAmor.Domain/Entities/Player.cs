using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public class Player
{
    // TODO: Add an in game id to save sending email each time
    public Guid GameId { get; set; }
    public required string Username { get; set; }
    public required string UserEmail { get; set; }
    public ICollection<CardType> PlayedCards { get; set; } = [];
    public required IList<CardType> HoldingCards { get; set; }
    public int Score { get; set; } = 0;
    public bool Protected { get; set; } = false;
}
