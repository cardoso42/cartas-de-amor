using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Priest : Card
{
    public Priest()
    {
        Name = "Priest";
        Value = 2;
    }

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override ICollection<CardActionRequirements> GetCardActionRequirements()
    {
        return [CardActionRequirements.SelectPlayer];
    }
}