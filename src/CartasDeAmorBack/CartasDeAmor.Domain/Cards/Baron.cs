using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;

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
            throw new CardRequirementsNotMetException("Target player must be specified for Baron action.");
        }

        if (targetPlayer.CanBeTargeted() == false)
        {
            throw new PlayerProtectedException("Target player cannot be targeted by the Baron action.", targetPlayer.UserEmail);
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

    public override bool MustBePlayed(Player invokerPlayer)
    {
        return false;
    }
}