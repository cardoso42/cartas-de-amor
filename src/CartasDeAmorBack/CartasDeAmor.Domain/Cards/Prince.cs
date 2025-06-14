using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Prince : Card
{
    public Prince()
    {
        Name = "Prince";
        Value = 5;
    }

    public override CardType CardType => CardType.Prince;

    public override void Play(Player currentPlayer, Game game)
    {
        
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