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

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override ICollection<CardActionRequirements> GetCardActionRequirements()
    {
        return [CardActionRequirements.None];
    }
}