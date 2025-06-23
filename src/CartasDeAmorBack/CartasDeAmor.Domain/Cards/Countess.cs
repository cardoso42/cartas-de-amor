using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;

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

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Has no effect
        return new CardResult()
        {
            SpecialMessages = [EventMessageFactory.PlayCard(invokerPlayer.UserEmail, CardType)]
        };
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