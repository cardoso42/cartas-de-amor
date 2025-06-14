using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Chanceller : Card
{
    public Chanceller()
    {
        Name = "Chanceller";
        Value = 6;
    }

    public override CardType CardType => CardType.Chanceller;

    public override void Play(Player currentPlayer, Game game)
    {

    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }
}