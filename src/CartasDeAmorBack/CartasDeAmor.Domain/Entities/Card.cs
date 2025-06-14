using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public abstract class Card
{
    public string Name { get; internal set; } = string.Empty;
    public int Value { get; internal set; }
    public abstract void Play(Player currentPlayer, Game game);
    public abstract CardRequirements? GetCardActionRequirements();
    public abstract CardType CardType { get; }
}
