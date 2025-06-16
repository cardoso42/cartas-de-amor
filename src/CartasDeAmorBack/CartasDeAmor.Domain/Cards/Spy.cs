using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Spy : Card
{
    public Spy()
    {
        Name = "Spy";
        Value = 0;
    }

    public override CardType CardType => CardType.Spy;

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // No immediate effect
        return CardActionResults.None;
    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }

    public override bool MustBePlayed(Player invokerPlayer)
    {
        return false;
    }
}