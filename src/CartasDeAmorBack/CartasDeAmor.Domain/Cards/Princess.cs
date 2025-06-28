using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Events;

namespace CartasDeAmor.Domain.Cards;

public class Princess : Card
{
    public Princess()
    {
        Name = "Princess";
        Value = 9;
    }

    public override CardType CardType => CardType.Princess;

    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) => false);

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Player loses round
        invokerPlayer.Eliminate();
        return new CardResult
        {
            Events =
            [
                new PlayCardEvent(invokerPlayer.UserEmail, CardType),
                new PlayerEliminatedEvent(invokerPlayer.UserEmail)
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