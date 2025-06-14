using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Countess : Card
{
    public Countess()
    {
        Name = "Countess";
        Value = 8;
    }

    public override CardType CardType => CardType.Countess;

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }
}