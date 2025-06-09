using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Cards;

public class Guard : Card
{
    public Guard()
    {
        Name = "Guard";
        Value = 1;
    }

    public override CardType CardType => CardType.Guard;

    public override void Play(Player currentPlayer, Game game)
    {
        
    }
    
    public override ICollection<CardActionRequirements> GetCardActionRequirements()
    {
        return [
            CardActionRequirements.SelectPlayer,
            CardActionRequirements.SelectCardType
        ];
    }
}