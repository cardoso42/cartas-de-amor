using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Factories;

namespace CartasDeAmor.Domain.Cards;

public class Guard : Card
{
    public Guard()
    {
        Name = "Guard";
        Value = 1;
    }

    public override CardType CardType => CardType.Guard;
    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) => false);

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
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

        if (targetPlayer.CanBeTargeted() == false)
        {
            throw new PlayerProtectedException("Target player cannot be targeted at this time.", targetPlayer.UserEmail);
        }

        var result = new CardResult()
        {
            SpecialMessages =
            [
                MessageFactory.PlayCard(invokerPlayer.UserEmail, CardType),
                MessageFactory.GuessCard(invokerPlayer.UserEmail, targetPlayer.UserEmail, targetCardType.Value)
            ]
        };

        if (targetPlayer.HasCard(targetCardType.Value))
        {
            targetPlayer.Eliminate();
            result.SpecialMessages.Add(MessageFactory.PlayerEliminated(targetPlayer.UserEmail));
        }

        return result;
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

    public override bool MustBePlayed(Player invokerPlayer)
    {
        return false;
    }
}