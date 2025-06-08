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

    public override void Play(Player currentPlayer, Game game)
    {

    }
    
    public override ICollection<CardActionRequirements> GetCardActionRequirements()
    {
        return [
            CardActionRequirements.DrawCard,
            CardActionRequirements.DrawCard,
            CardActionRequirements.SelectCard
        ];
    }
}