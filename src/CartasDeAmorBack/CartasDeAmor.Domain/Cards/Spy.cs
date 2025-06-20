using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;

namespace CartasDeAmor.Domain.Cards;

public class Spy : Card
{
    public Spy()
    {
        Name = "Spy";
        Value = 0;
    }

    public override CardType CardType => CardType.Spy;

    public override CardResult Play(Game game, Player invokerPlayer, Player? targetPlayer, CardType? targetCardType)
    {
        // No immediate effect
        return new CardResult
        {
            SpecialMessages = [MessageFactory.PlayCard(invokerPlayer.UserEmail, CardType)]
        };
    }
    
    public override CardRequirements? GetCardActionRequirements()
    {
        return null;
    }

    public override bool MustBePlayed(Player invokerPlayer)
    {
        return false;
    }

    public override Func<Game, Player, bool> ConditionForExtraPoint => new((game, player) =>
    {
        if (player.HasPlayedCard(CardType) == false) return false;

        var activeGamePlayers = game.GetActivePlayers().Where(p => p.UserEmail != player.UserEmail).ToList();

        foreach (var activePlayer in activeGamePlayers)
        {
            if (activePlayer.HasPlayedCard(CardType))
            {
                // If any other player has a Spy card, the player does not get an extra point
                return false;
            }
        }

        return true;
    });
}