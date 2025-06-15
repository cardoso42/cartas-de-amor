using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Baron : Card
{
    public override CardType CardType => CardType.Baron;

    public Baron()
    {
        Name = "Baron";
        Value = 3;
    }

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Compare both the current player's card and the target player's card.
        // Whoever has the lower value loses the round

        if (targetPlayer == null)
        {
            throw new ArgumentNullException(nameof(targetPlayer), "Target player must be specified for Baron action.");
        }

        if (targetPlayer.IsProtected())
        {
            throw new InvalidOperationException("Target player is protected and cannot be affected by Baron action.");
        }

        var invokerCard = invokerPlayer.GetCard();
        var targetCard = targetPlayer.GetCard();

        if (invokerCard > targetCard)
        {
            targetPlayer.Eliminate();
            return CardActionResults.PlayerEliminated;
        }
        else if (invokerCard < targetCard)
        {
            invokerPlayer.Eliminate();
            return CardActionResults.PlayerEliminated;
        }

        return CardActionResults.None;
    }

    public override CardRequirements? GetCardActionRequirements()
    {
        return new()
        {
            CardType = CardType,
            IsTargetRequired = true,
            CanChooseSelf = false,
            IsCardTypeRequired = false
        };
    }
}