using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Factories;

namespace CartasDeAmor.Domain.Cards;

public class Chanceller : Card
{
    public Chanceller()
    {
        Name = "Chanceller";
        Value = 6;
    }

    public override CardType CardType => CardType.Chanceller;
    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) => false);

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // Player draws two cards from the deck and chooses one to keep.

        var result = new CardResult();
        result.SpecialMessages.Add(EventMessageFactory.PlayCard(invokerPlayer.UserEmail, CardType));
        
        try
        {
            for (int i = 0; i < 2; i++)
            {
                var drawnCard = game.DrawCard();
                invokerPlayer.HandCard(drawnCard);
                result.SpecialMessages.Add(EventMessageFactory.DrawCard(invokerPlayer.UserEmail));

                // This is done inside the loop in case the deck is empty
                // It doesn't really make sense to make the player choose if it has only one card
                result.ShouldAdvanceTurn = false;
            }
        }
        catch (EmptyDeckException)
        {
            // According to the rules, if the card deck is empty when playing a
            // Chanceller card, the player just draws what is available (if any)
            // So, we don't need to handle this case specifically.
        }

        if (result.ShouldAdvanceTurn == false)
        {
            // The player was able to draw cards, so we prompt them to choose one
            result.SpecialMessages.Add(EventMessageFactory.ChooseCard(invokerPlayer.UserEmail));
        }

        return result;
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