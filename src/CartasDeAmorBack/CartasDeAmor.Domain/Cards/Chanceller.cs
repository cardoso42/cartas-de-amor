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

    public override CardType CardType => CardType.Chanceller;

    public override CardActionResults Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Player draws two cards from the deck and chooses one to keep.
        
        for (int i = 0; i < 2; i++)
        {
            var drawnCard = game.DrawCard();
            if (drawnCard == null) break;
            invokerPlayer.HandCard(drawnCard.Value);
        }

        return CardActionResults.ChooseCard;

    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }
}