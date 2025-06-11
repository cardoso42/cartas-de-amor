using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Servant : Card
{
    public Servant()
    {
        Name = "Servant";
        Value = 4;
    }

    public override CardType CardType => CardType.Servant;

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override ICollection<CardActionRequirements> GetCardActionRequirements()
    {
        return [CardActionRequirements.None];
    }
}