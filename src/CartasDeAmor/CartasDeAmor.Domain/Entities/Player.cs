namespace CartasDeAmor.Domain.Entities;

public class Player
{
    public string UserEmail { get; set; } = string.Empty;
    public Guid GameId { get; set; }
    public ICollection<Card> PlayedCards { get; set; } = new List<Card>();
    public Card HoldingCard { get; set; } = null!;
    public int Score { get; set; } = 0;
    public bool Protected { get; set; } = false;
}
