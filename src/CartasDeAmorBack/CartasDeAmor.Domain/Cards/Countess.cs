using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Countess : Card
{
    public Countess()
    {
        Name = "Countess";
        Value = 8;
    }

    public override CardType CardType => CardType.Countess;
    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) => false);

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Has no effect
        return CardActionResults.None;
    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }

    public override bool MustBePlayed(Player invokerPlayer)
    {
        return invokerPlayer.HasCard(CardType.King) || invokerPlayer.HasCard(CardType.Prince);
    }
}