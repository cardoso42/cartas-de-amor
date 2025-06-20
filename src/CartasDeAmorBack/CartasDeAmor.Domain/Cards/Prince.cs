using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Factories;

namespace CartasDeAmor.Domain.Cards;

public class Prince : Card
{
    public Prince()
    {
        Name = "Prince";
        Value = 5;
    }

    public override CardType CardType => CardType.Prince;
    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) => false);

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // The player chooses another player and that player must discard their hand and draw a new card.

        if (targetPlayer == null)
        {
            throw new CardRequirementsNotMetException("Target player is required for Prince card action.");
        }

        if (targetPlayer.CanBeTargeted() == false)
        {
            throw new PlayerProtectedException("Target player cannot be targeted by the Prince card.", targetPlayer.UserEmail);
        }

        // Discard the target player's hand
        var discardedCard = targetPlayer.GetCard();
        targetPlayer.PlayCard(discardedCard);

        var result = new CardResult
        {
            SpecialMessages =
            [
                MessageFactory.PlayCard(invokerPlayer.UserEmail, CardType),
                MessageFactory.DiscardCard(targetPlayer.UserEmail, discardedCard)
            ]
        };

        if (discardedCard == CardType.Princess)
        {
            targetPlayer.Eliminate();
            result.SpecialMessages.Add(MessageFactory.PlayerEliminated(targetPlayer.UserEmail));
            return result;
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
        result.SpecialMessages.Add(MessageFactory.DrawCard(targetPlayer.UserEmail));

        return result;
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

    public override bool MustBePlayed(Player invokerPlayer)
    {
        return false;
    }
}