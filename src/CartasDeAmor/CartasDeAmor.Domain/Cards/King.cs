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

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override ICollection<CardActionRequirements> GetCardActionRequirements()
    {
        return [CardActionRequirements.SelectPlayer];
    }
}