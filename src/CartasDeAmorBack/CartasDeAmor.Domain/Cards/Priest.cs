using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

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
            throw new ArgumentNullException(nameof(targetPlayer), "Target player must be specified for Priest card action.");
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