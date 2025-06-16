using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;

namespace CartasDeAmor.Domain.Cards;

public class Prince : Card
{
    public Prince()
    {
        Name = "Prince";
        Value = 5;
    }

    public override CardType CardType => CardType.Prince;

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // The player chooses another player and that player must discard their hand and draw a new card.

        if (targetPlayer == null)
        {
            throw new ArgumentException("Target player is required for Prince card action.");
        }

        // Discard the target player's hand
        var discardedCard = targetPlayer.GetCard();
        targetPlayer.PlayCard(discardedCard);

        if (discardedCard == CardType.Princess)
        {
            targetPlayer.Eliminate();
            return CardActionResults.PlayerEliminated;
        }

        CardType newCard;
        try
        {
            newCard = game.DrawCard();
        }
        catch (EmptyDeckException)
        {
            newCard = game.GetReservedCard();
        }

        targetPlayer.HandCard(newCard);

        return CardActionResults.DiscardAndDrawCard;
    }

    public override CardRequirements? GetCardActionRequirements()
    {
        return new()
        {
            CardType = CardType,
            IsTargetRequired = true,
            CanChooseSelf = true,
            IsCardTypeRequired = false
        };
    }
}