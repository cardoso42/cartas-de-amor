using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Spy : Card
{
    public Spy()
    {
        Name = "Spy";
        Value = 0;
    }

    public override CardType CardType => CardType.Spy;

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }
}