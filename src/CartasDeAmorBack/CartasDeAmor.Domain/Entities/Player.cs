using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public class Player
{
    public int Id { get; set; }
    public Guid GameId { get; set; }
    public required string Username { get; set; }
    public required string UserEmail { get; set; }
    public IList<CardType> PlayedCards { get; set; } = [];
    public required IList<CardType> HoldingCards { get; set; }
    public int Score { get; set; } = 0;
    public PlayerStatus Status { get; set; } = PlayerStatus.Active;

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
    ///  Marks the player as eliminated, transferring all their cards to played cards
    /// </summary>
    public void Eliminate()
    {
        foreach (var card in HoldingCards)
        {
            PlayedCards.Add(card);
        }

        HoldingCards.Clear();

        Status = PlayerStatus.Eliminated;
    }

    /// <summary>
    /// Gets the first card from the player's hand
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the player has no cards in hand</exception>
    /// <returns>The first card in the player's hand</returns>
    public CardType GetCard()
    {
        if (HoldingCards.Count == 0)
            throw new InvalidOperationException("Player has no cards in hand.");

        return HoldingCards.First();
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
    /// Removes the first card from the player's hand
    /// </summary>
    public CardType RemoveCard()
    {
        if (HoldingCards.Count == 0)
            throw new InvalidOperationException("Player has no cards in hand.");

        var card = HoldingCards.First();
        HoldingCards.Remove(card);
        return card;
    }

    /// <summary>
    /// Adds a card to the player's hand
    /// </summary>
    public void HandCard(CardType cardType)
    {
        HoldingCards.Add(cardType);
    }

    public void HandCards(IList<CardType> cardTypes)
    {
        foreach (var cardType in cardTypes)
        {
            HoldingCards.Add(cardType);
        }
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

    public bool RevertPlayCard(CardType cardType)
    {
        if (PlayedCards.Contains(cardType))
        {
            PlayedCards.Remove(cardType);
            HoldingCards.Add(cardType);
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
        return Status == PlayerStatus.Protected;
    }
    
    public bool IsInGame()
    {
        return Status == PlayerStatus.Active || Status == PlayerStatus.Protected;
    }

    /// <summary>
    /// Sets the player's protection status
    /// </summary>
    public void SetProtection(bool isProtected)
    {
        if (Status != PlayerStatus.Active && Status != PlayerStatus.Protected)
            throw new InvalidOperationException("Cannot change protection status of an eliminated or disconnected player.");

        Status = isProtected ? PlayerStatus.Protected : PlayerStatus.Active;
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
        PlayedCards.Clear();

        if (Status == PlayerStatus.Protected || Status == PlayerStatus.Eliminated)
        {
            Status = PlayerStatus.Active;
        }
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

    public IList<CardType> TakeHoldingCards()
    {
        var holdingCardsCopy = GetHandCopy();
        HoldingCards.Clear();
        return holdingCardsCopy;
    }

    /// <summary>
    /// Gets a copy of the player's current hand
    /// </summary>
    public IList<CardType> GetHandCopy()
    {
        return HoldingCards.ToList();
    }

    /// <summary>
    /// Checks if the player has won the game (reached a specific score)
    /// </summary>
    public bool HasWon(int winningScore = 7)
    {
        return Score >= winningScore;
    }
}
