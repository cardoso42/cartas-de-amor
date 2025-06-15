using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class King : Card
{
    public King()
    {
        Name = "King";
        Value = 7;
    }

    public override CardType CardType => CardType.King;

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        if (targetPlayer == null)
        {
            throw new ArgumentNullException(nameof(targetPlayer), "Target player must be specified for King card action.");
        }

        // Swap the invoker's hand with the target player's hand
        var targetPlayerCard = targetPlayer.RemoveCard();
        var invokerPlayerCard = invokerPlayer.RemoveCard();
        invokerPlayer.HandCard(targetPlayerCard);
        targetPlayer.HandCard(invokerPlayerCard);

        return CardActionResults.SwitchCards;
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