using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Events;

namespace CartasDeAmor.Domain.Cards;

public class King : Card
{
    public King()
    {
        Name = "King";
        Value = 7;
    }

    public override CardType CardType => CardType.King;
    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) => false);

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        if (targetPlayer == null)
        {
            throw new CardRequirementsNotMetException("Target player must be specified for King card action.");
        }

        if (targetPlayer.CanBeTargeted() == false)
        {
            throw new PlayerProtectedException("Target player cannot be targeted by the King card.", targetPlayer.UserEmail);
        }

        // Swap the invoker's hand with the target player's hand
        var targetPlayerCard = targetPlayer.TakeHoldingCards();
        var invokerPlayerCard = invokerPlayer.TakeHoldingCards();
        invokerPlayer.HandCards(targetPlayerCard);
        targetPlayer.HandCards(invokerPlayerCard);

        return new CardResult
        {
            Events =
            [
                new PlayCardEvent(invokerPlayer.UserEmail, CardType),
                new SwitchCardsEvent(invokerPlayer.UserEmail, targetPlayer.UserEmail)
            ]
        };
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