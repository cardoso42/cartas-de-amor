using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;

namespace CartasDeAmor.Domain.Cards;

public class Servant : Card
{
    public Servant()
    {
        Name = "Servant";
        Value = 4;
    }

    public override CardType CardType => CardType.Servant;

    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) => false);

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Player is protected from being targeted by other players' cards until their next turn.
        invokerPlayer.SetProtection(true);
        
        return new CardResult 
        {
            SpecialMessages =
            [
                MessageFactory.PlayCard(invokerPlayer.UserEmail, CardType),
                MessageFactory.PlayerProtected(invokerPlayer.UserEmail)
            ]
        };
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