using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;

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

        try
        {
            for (int i = 0; i < 2; i++)
            {
                var drawnCard = game.DrawCard();
                invokerPlayer.HandCard(drawnCard);
            }
        }
        catch (EmptyDeckException)
        {
            // According to the rules, if the card deck is empty when playing a
            // Chanceller card, the player just draws what is available (if any)
            // So, we don't need to handle this case specifically.
        }

        return CardActionResults.ChooseCard;
    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }

    public override bool MustBePlayed(Player invokerPlayer)
    {
        return false;
    }
}