using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.Extensions;

public static class PlayerExtension
{
    /// <summary>
    /// Converts a Player entity to a PlayerStatusDto
    /// </summary>
    public static PlayerStatusDto ToPlayerStatusDto(this Player player, PlayerStatus status = PlayerStatus.Playing)
    {
        return new PlayerStatusDto
        {
            UserEmail = player.UserEmail,
            Username = player.Username,
            Status = status,
            IsProtected = player.Protected,
            Score = player.Score,
            CardsInHand = player.HoldingCards.Count
        };
    }

    /// <summary>
    /// Checks if the player has a specific card type in their hand
    /// </summary>
    public static bool HasCard(this Player player, CardType cardType)
    {
        return player.HoldingCards.Contains(cardType);
    }

    /// <summary>
    /// Checks if the player has any cards in their hand
    /// </summary>
    public static bool HasCards(this Player player)
    {
        return player.HoldingCards.Count > 0;
    }

    /// <summary>
    /// Gets the highest value card in the player's hand
    /// </summary>
    public static CardType? GetHighestCard(this Player player)
    {
        if (!player.HasCards())
            return null;

        return player.HoldingCards.OrderByDescending(card => (int)card).FirstOrDefault();
    }

    /// <summary>
    /// Gets the lowest value card in the player's hand
    /// </summary>
    public static CardType? GetLowestCard(this Player player)
    {
        if (!player.HasCards())
            return null;

        return player.HoldingCards.OrderBy(card => (int)card).FirstOrDefault();
    }

    /// <summary>
    /// Removes a specific card from the player's hand
    /// </summary>
    public static bool RemoveCard(this Player player, CardType cardType)
    {
        if (player.HasCard(cardType))
        {
            player.HoldingCards.Remove(cardType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a card to the player's hand
    /// </summary>
    public static void HandCard(this Player player, CardType cardType)
    {
        player.HoldingCards.Add(cardType);
    }

    /// <summary>
    /// Plays a card (removes from hand and adds to played cards)
    /// </summary>
    public static bool PlayCard(this Player player, CardType cardType)
    {
        if (player.RemoveCard(cardType))
        {
            player.PlayedCards.Add(cardType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the player is eliminated (has no cards)
    /// </summary>
    public static bool IsEliminated(this Player player)
    {
        return !player.HasCards();
    }

    /// <summary>
    /// Checks if the player is protected from card effects
    /// </summary>
    public static bool IsProtected(this Player player)
    {
        return player.Protected;
    }

    /// <summary>
    /// Sets the player's protection status
    /// </summary>
    public static void SetProtection(this Player player, bool isProtected)
    {
        player.Protected = isProtected;
    }

    /// <summary>
    /// Increments the player's score by a specified amount
    /// </summary>
    public static void AddScore(this Player player, int points = 1)
    {
        player.Score += points;
    }

    /// <summary>
    /// Resets the player's hand and protection status for a new round
    /// </summary>
    public static void ResetForNewRound(this Player player)
    {
        player.HoldingCards.Clear();
        player.Protected = false;
    }

    /// <summary>
    /// Gets all cards the player has played this game
    /// </summary>
    public static IEnumerable<CardType> GetPlayedCards(this Player player)
    {
        return player.PlayedCards.AsEnumerable();
    }

    /// <summary>
    /// Checks if the player has played a specific card type
    /// </summary>
    public static bool HasPlayedCard(this Player player, CardType cardType)
    {
        return player.PlayedCards.Contains(cardType);
    }

    /// <summary>
    /// Gets the count of how many times a specific card has been played
    /// </summary>
    public static int GetPlayedCardCount(this Player player, CardType cardType)
    {
        return player.PlayedCards.Count(card => card == cardType);
    }

    /// <summary>
    /// Checks if the player can be targeted by card effects (not protected and has cards)
    /// </summary>
    public static bool CanBeTargeted(this Player player)
    {
        return !player.IsProtected() && !player.IsEliminated();
    }

    /// <summary>
    /// Gets a copy of the player's current hand
    /// </summary>
    public static IList<CardType> GetHandCopy(this Player player)
    {
        return new List<CardType>(player.HoldingCards);
    }

    /// <summary>
    /// Checks if the player has won the game (reached a specific score)
    /// </summary>
    public static bool HasWon(this Player player, int winningScore = 7)
    {
        return player.Score >= winningScore;
    }
}