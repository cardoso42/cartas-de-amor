using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Events;

namespace CartasDeAmor.Domain.Cards;

public class Baron : Card
{
    public override CardType CardType => CardType.Baron;
    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) => false);

    public Baron()
    {
        Name = "Baron";
        Value = 3;
    }

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
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

        var result = new CardResult();
        result.Events.Add(new PlayCardEvent(invokerPlayer.UserEmail, CardType));

        var invokerCard = invokerPlayer.GetCard();
        var targetCard = targetPlayer.GetCard();

        result.Events.Add(new CompareCardsEvent(invokerPlayer.UserEmail, targetPlayer.UserEmail));

        if (invokerCard > targetCard)
        {
            targetPlayer.Eliminate();
            result.Events.Add(new PlayerEliminatedEvent(targetPlayer.UserEmail));
        }
        else if (invokerCard < targetCard)
        {
            invokerPlayer.Eliminate();
            result.Events.Add(new PlayerEliminatedEvent(invokerPlayer.UserEmail));
        }
        else
        {
            result.Events.Add(new ComparisonTieEvent(invokerPlayer.UserEmail, targetPlayer.UserEmail));
        }
        
        return result;
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