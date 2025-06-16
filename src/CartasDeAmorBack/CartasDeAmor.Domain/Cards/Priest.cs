using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;

namespace CartasDeAmor.Domain.Cards;

public class Priest : Card
{
    public Priest()
    {
        Name = "Priest";
        Value = 2;
    }

    public override CardType CardType => CardType.Priest;

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        if (targetPlayer == null)
        {
            throw new CardRequirementsNotMetException("Target player must be specified for Priest card action.");
        }

        if (targetPlayer.CanBeTargeted() == false)
        {
            throw new PlayerProtectedException("Target player cannot be targeted by the Priest card.", targetPlayer.UserEmail);
        }

        return CardActionResults.ShowCard;
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