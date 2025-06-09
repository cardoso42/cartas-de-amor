using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Princess : Card
{
    public Princess()
    {
        Name = "Princess";
        Value = 9;
    }

    public override CardType CardType => CardType.Princess;

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override ICollection<CardActionRequirements> GetCardActionRequirements()
    {
        return [CardActionRequirements.None];
    }
}