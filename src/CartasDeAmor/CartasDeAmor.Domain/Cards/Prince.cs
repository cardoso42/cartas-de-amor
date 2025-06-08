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

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override ICollection<CardActionRequirements> GetCardActionRequirements()
    {
        return [CardActionRequirements.SelectPlayer];
    }
}