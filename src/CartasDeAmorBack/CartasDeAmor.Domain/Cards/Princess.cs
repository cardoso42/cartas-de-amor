using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

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

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Player loses round
        invokerPlayer.Eliminate();
        return CardActionResults.PlayerEliminated;
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