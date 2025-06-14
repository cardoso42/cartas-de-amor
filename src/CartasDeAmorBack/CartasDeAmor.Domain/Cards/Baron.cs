using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Baron : Card
{
    public override CardType CardType => CardType.Baron;

    public Baron()
    {
        Name = "Baron";
        Value = 3;
    }

    public override void Play(Player currentPlayer, Game game)
    {

    }

    // TODO: Move this to database stored data

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