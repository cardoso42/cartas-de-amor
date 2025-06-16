using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;

namespace CartasDeAmor.Domain.Cards;

public class Guard : Card
{
    public Guard()
    {
        Name = "Guard";
        Value = 1;
    }

    public override CardType CardType => CardType.Guard;

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // If the player guesses the card type of the target player correctly, the target player is eliminated.

        if (targetPlayer == null || targetCardType == null)
        {
            throw new CardRequirementsNotMetException("Target player and card type must be provided for Guard action.");
        }

        if (targetPlayer.IsEliminated())
        {
            throw new InvalidOperationException("Target player is already eliminated.");
        }

        if (!targetPlayer.CanBeTargeted())
        {
            throw new InvalidOperationException("Target player cannot be targeted at this time.");
        }

        if (targetPlayer.HasCard(targetCardType.Value))
        {
            targetPlayer.Eliminate();
            return CardActionResults.PlayerEliminated;
        }
        else
        {
            return CardActionResults.None;
        }
    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return new CardRequirements
        {
            CardType = CardType,
            IsTargetRequired = true,
            CanChooseSelf = false,
            IsCardTypeRequired = true,
            CanChooseEqualCardType = false,
        };
    }
}