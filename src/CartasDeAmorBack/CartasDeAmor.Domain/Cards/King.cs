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

    public override void Play(Player currentPlayer, Game game)
    {
        
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