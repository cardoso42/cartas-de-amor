using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public abstract class Card
{
    public string Name { get; internal set; }
    public int Value { get; internal set; }
    public abstract void Play(Player currentPlayer, Game game);
    public abstract ICollection<CardActionRequirements> GetCardActionRequirements();
}
