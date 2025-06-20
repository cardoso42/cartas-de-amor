using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Factories;

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
        result.SpecialMessages.Add(MessageFactory.PlayCard(invokerPlayer.UserEmail, CardType));

        var invokerCard = invokerPlayer.GetCard();
        var targetCard = targetPlayer.GetCard();

        result.SpecialMessages.Add(MessageFactory.CompareCards(invokerPlayer.UserEmail, targetPlayer.UserEmail));

        if (invokerCard > targetCard)
        {
            targetPlayer.Eliminate();
            result.SpecialMessages.Add(MessageFactory.PlayerEliminated(targetPlayer.UserEmail));
        }
        else if (invokerCard < targetCard)
        {
            invokerPlayer.Eliminate();
            result.SpecialMessages.Add(MessageFactory.PlayerEliminated(invokerPlayer.UserEmail));
        }
        else
        {
            result.SpecialMessages.Add(MessageFactory.ComparisonTie(invokerPlayer.UserEmail, targetPlayer.UserEmail));
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