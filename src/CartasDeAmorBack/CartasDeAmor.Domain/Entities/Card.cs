using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public abstract class Card
{
    public string Name { get; internal set; } = string.Empty;
    public int Value { get; internal set; }
    public abstract CardType CardType { get; }
    public abstract bool MustBePlayed(Player invokerPlayer);
    public abstract CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType);
    public abstract CardRequirements? GetCardActionRequirements();
    public abstract Func<Game, Player, bool> ConditionForExtraPoint { get; }
}
