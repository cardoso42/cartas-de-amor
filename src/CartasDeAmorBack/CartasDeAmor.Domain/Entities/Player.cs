using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public class Player
{
    public int Id { get; set; }
    public Guid GameId { get; set; }
    public required string Username { get; set; }
    public required string UserEmail { get; set; }
    public ICollection<CardType> PlayedCards { get; set; } = [];
    public required IList<CardType> HoldingCards { get; set; }
    public int Score { get; set; } = 0;
    public bool Protected { get; set; } = false;

    // /// <summary>
    // /// Converts a Player entity to a PlayerStatusDto
    // /// </summary>
    // public PlayerStatusDto ToPlayerStatusDto(PlayerStatus status = PlayerStatus.Playing)
    // {
    //     return new PlayerStatusDto
    //     {
    //         UserEmail = UserEmail,
    //         Username = Username,
    //         Status = status,
    //         IsProtected = Protected,
    //         Score = Score,
    //         CardsInHand = HoldingCards.Count
    //     };
    // }

    // /// <summary>
    // /// Converts a Player entity to a PlayerUpdateDto
    // /// </summary>
    // public PlayerUpdateDto ToPlayerUpdateDto()
    // {
    //     return new PlayerUpdateDto
    //     {
    //         UserEmail = UserEmail,
    //         IsProtected = Protected,
    //         HoldingCards = HoldingCards.ToList(),
    //         PlayedCards = PlayedCards.ToList(),
    //         Score = Score
    //     };
    // }

    /// <summary>
    /// Checks if the player has a specific card type in their hand
    /// </summary>
    public bool HasCard(CardType cardType)
    {
        return HoldingCards.Contains(cardType);
    }

    /// <summary>
    /// Checks if the player has any cards in their hand
    /// </summary>
    public bool HasCards()
    {
        return HoldingCards.Count > 0;
    }

    /// <summary>
    /// Gets the highest value card in the player's hand
    /// </summary>
    public CardType? GetHighestCard()
    {
        if (!HasCards())
            return null;

        return HoldingCards.OrderByDescending(card => (int)card).FirstOrDefault();
    }

    /// <summary>
    /// Gets the lowest value card in the player's hand
    /// </summary>
    public CardType? GetLowestCard()
    {
        if (!HasCards())
            return null;

        return HoldingCards.OrderBy(card => (int)card).FirstOrDefault();
    }

    /// <summary>
    /// Removes a specific card from the player's hand
    /// </summary>
    public bool RemoveCard(CardType cardType)
    {
        if (HasCard(cardType))
        {
            HoldingCards.Remove(cardType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a card to the player's hand
    /// </summary>
    public void HandCard(CardType cardType)
    {
        HoldingCards.Add(cardType);
    }

    /// <summary>
    /// Plays a card (removes from hand and adds to played cards)
    /// </summary>
    public bool PlayCard(CardType cardType)
    {
        if (RemoveCard(cardType))
        {
            PlayedCards.Add(cardType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the player is eliminated (has no cards)
    /// </summary>
    public bool IsEliminated()
    {
        return !HasCards();
    }

    /// <summary>
    /// Checks if the player is protected from card effects
    /// </summary>
    public bool IsProtected()
    {
        return Protected;
    }

    /// <summary>
    /// Sets the player's protection status
    /// </summary>
    public void SetProtection(bool isProtected)
    {
        Protected = isProtected;
    }

    /// <summary>
    /// Increments the player's score by a specified amount
    /// </summary>
    public void AddScore(int points = 1)
    {
        Score += points;
    }

    /// <summary>
    /// Resets the player's hand and protection status for a new round
    /// </summary>
    public void ResetForNewRound()
    {
        HoldingCards.Clear();
        Protected = false;
    }

    /// <summary>
    /// Gets all cards the player has played this game
    /// </summary>
    public IEnumerable<CardType> GetPlayedCards()
    {
        return PlayedCards.AsEnumerable();
    }

    /// <summary>
    /// Checks if the player has played a specific card type
    /// </summary>
    public bool HasPlayedCard(CardType cardType)
    {
        return PlayedCards.Contains(cardType);
    }

    /// <summary>
    /// Gets the count of how many times a specific card has been played
    /// </summary>
    public int GetPlayedCardCount(CardType cardType)
    {
        return PlayedCards.Count(card => card == cardType);
    }

    /// <summary>
    /// Checks if the player can be targeted by card effects (not protected and has cards)
    /// </summary>
    public bool CanBeTargeted()
    {
        return !IsProtected() && !IsEliminated();
    }

    /// <summary>
    /// Gets a copy of the player's current hand
    /// </summary>
    public IList<CardType> GetHandCopy()
    {
        return new List<CardType>(HoldingCards);
    }

    /// <summary>
    /// Checks if the player has won the game (reached a specific score)
    /// </summary>
    public bool HasWon(int winningScore = 7)
    {
        return Score >= winningScore;
    }
}
