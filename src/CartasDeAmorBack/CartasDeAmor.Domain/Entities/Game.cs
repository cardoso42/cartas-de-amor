using System.Diagnostics.Contracts;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Factories;

namespace CartasDeAmor.Domain.Entities;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Password { get; set; } = null;
    public bool PasswordProtected => !string.IsNullOrEmpty(Password);
    public required string HostEmail { get; set; }
    public IList<Player> Players { get; set; } = [];
    public IList<CardType> CardsDeck { get; set; } = [];
    public CardType? ReservedCard { get; set; }
    public int CurrentPlayerIndex { get; set; } = 0;
    public int MaxTokens { get; set; } = 3;
    public GameStateEnum GameState { get; set; } = GameStateEnum.WaitingForPlayers;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the current player in the game
    /// </summary>
    public Player? GetCurrentPlayer()
    {
        if (Players.Count == 0 || CurrentPlayerIndex >= Players.Count)
            return null;

        return Players.ElementAt(CurrentPlayerIndex);
    }

    private int GetNextPlayerIndex()
    {
        if (Players.Count == 0)
            throw new InvalidOperationException("No players in the game.");

        var nextIndex = (CurrentPlayerIndex + 1) % Players.Count;

        // Skip eliminated players
        while (nextIndex != CurrentPlayerIndex && Players.ElementAt(nextIndex).IsEliminated())
        {
            nextIndex = (nextIndex + 1) % Players.Count;
        }

        return nextIndex;
    }

    /// <summary>
    /// Gets the next player in turn order
    /// </summary>
    public Player? GetNextPlayer()
    {
        try
        {
            var nextIndex = GetNextPlayerIndex();
            return Players.ElementAt(nextIndex);
        }
        catch (InvalidOperationException)
        {
            return null; // No players available
        }
    }

    /// <summary>
    /// Advances to the next player's turn
    /// </summary>
    public void AdvanceToNextPlayer()
    {
        try
        {
            CurrentPlayerIndex = GetNextPlayerIndex();
            UpdatedAt = DateTime.UtcNow;
        }
        catch (InvalidOperationException ex)
        {
            throw new GameException("Cannot advance to next player: " + ex.Message);
        }
    }

    /// <summary>
    /// Checks if the game is full (reached maximum players)
    /// </summary>
    public bool IsFull(int maxPlayers = 6)
    {
        return Players.Count >= maxPlayers;
    }

    /// <summary>
    /// Gets a player by their email
    /// </summary>
    public Player? GetPlayerByEmail(string email)
    {
        return Players.FirstOrDefault(p => p.UserEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a player with the given email exists in the game
    /// </summary>
    public bool HasPlayer(string email)
    {
        return GetPlayerByEmail(email) != null;
    }

    /// <summary>
    /// Checks if the game can be started (minimum players requirement)
    /// </summary>
    public bool CanStart(int minPlayers = 2)
    {
        return GameState == GameStateEnum.WaitingForPlayers && Players.Count >= minPlayers;
    }

    /// <summary>
    /// Checks if the game has started (not in WaitingForPlayers state)
    /// </summary>
    public bool HasStarted()
    {
        return GameState != GameStateEnum.WaitingForPlayers;
    }

    public bool IsActive()
    {
        return GameState != GameStateEnum.Finished && Players.Count > 0;
    }

    /// <summary>
    /// Gets the number of active (not eliminated) players
    /// </summary>
    public int GetActivePlayerCount()
    {
        return Players.Count(p => p.IsInGame());
    }

    /// <summary>
    /// Gets all active (not eliminated) players
    /// </summary>
    public IEnumerable<Player> GetActivePlayers()
    {
        return Players.Where(p => p.IsInGame());
    }

    /// <summary>
    /// Gets all eliminated players
    /// </summary>
    public IEnumerable<Player> GetEliminatedPlayers()
    {
        return Players.Where(p => p.Status == PlayerStatus.Eliminated);
    }

    /// <summary>
    /// Checks if the game round is over (only one player remaining or deck is empty)
    /// </summary>
    public bool IsRoundOver()
    {
        return GetActivePlayerCount() <= 1 || CardsDeck.Count == 0;
    }

    /// <summary>
    /// Gets the winner of the current round
    /// </summary>
    public IList<Player> GetRoundWinners()
    {
        if (GameState == GameStateEnum.WaitingForPlayers)
            throw new GameNotStartedException("Cannot determine round winner before the game has started.");

        if (IsRoundOver() == false)
            throw new GameException("Cannot determine round winner while the round is still ongoing.");

        var activePlayers = GetActivePlayers().ToList();

        if (activePlayers.Count == 1)
            return [activePlayers.First()];

        if (activePlayers.Count == 0)
            throw new GameException("No active players left to determine a winner.");

        // If multiple players remain, highest card wins (ties are allowed)
        var playersBiggestCards = activePlayers.Select(p => p.HoldingCards.Max());
        var gameBiggestCard = playersBiggestCards.Max();

        return activePlayers.Where(p => p.HasCard(gameBiggestCard)).ToList();
    }

    /// <summary>
    /// Draws a card from the deck
    /// </summary>
    public CardType DrawCard()
    {
        if (CardsDeck.Count == 0)
            throw new EmptyDeckException();

        var card = CardsDeck.First();
        CardsDeck.Remove(card);
        UpdatedAt = DateTime.UtcNow;

        return card;
    }

    public CardType GetReservedCard()
    {
        if (CardsDeck.Count > 0)
            throw new InvalidOperationException("Cannot get reserved card while deck still has cards.");

        return ReservedCard ?? throw new InvalidOperationException("No reserved card set. Please draw a card to set the reserved card.");
    }

    /// <summary>
    /// Shuffles the deck
    /// </summary>
    public void ShuffleDeck()
    {
        var random = new Random();
        var shuffledCards = CardsDeck.OrderBy(x => random.Next()).ToList();
        CardsDeck.Clear();

        foreach (var card in shuffledCards)
        {
            CardsDeck.Add(card);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new game deck with the standard Love Letter card distribution
    /// </summary>
    public void InitializeDeck()
    {
        CardsDeck.Clear();

        // Standard Love Letter deck composition
        CardsDeck.Add(CardType.Spy);         // 2 cards
        CardsDeck.Add(CardType.Spy);

        CardsDeck.Add(CardType.Guard);       // 6 cards
        CardsDeck.Add(CardType.Guard);
        CardsDeck.Add(CardType.Guard);
        CardsDeck.Add(CardType.Guard);
        CardsDeck.Add(CardType.Guard);
        CardsDeck.Add(CardType.Guard);

        CardsDeck.Add(CardType.Priest);      // 2 cards
        CardsDeck.Add(CardType.Priest);

        CardsDeck.Add(CardType.Baron);       // 2 cards
        CardsDeck.Add(CardType.Baron);

        CardsDeck.Add(CardType.Servant);     // 2 cards
        CardsDeck.Add(CardType.Servant);

        CardsDeck.Add(CardType.Prince);      // 2 cards
        CardsDeck.Add(CardType.Prince);

        CardsDeck.Add(CardType.Chanceller);  // 2 cards
        CardsDeck.Add(CardType.Chanceller);

        CardsDeck.Add(CardType.King);        // 1 card
        CardsDeck.Add(CardType.Countess);    // 1 card
        CardsDeck.Add(CardType.Princess);    // 1 card

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the game is over (a player reached the target score)
    /// </summary>
    public bool IsGameOver()
    {
        return Players.Any(p => p.Score >= MaxTokens);
    }

    /// <summary>
    /// Gets the overall winner of the game
    /// </summary>
    public IList<Player> GetGameWinner()
    {
        var maxTokens = Players.Max(p => p.Score);
        return Players.Where(p => p.Score >= maxTokens).ToList();
    }

    /// <summary>
    /// Hands a card to the specified player
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the player is not found in the game</exception>
    public Player HandCardToPlayer(string playerEmail)
    {
        var player = GetPlayerByEmail(playerEmail);
        if (player == null)
            throw new InvalidOperationException($"Player with email {playerEmail} not found in the ");

        var card = DrawCard();
        player.HandCard(card);

        return player;
    }

    public void ConfigureGame()
    {
        MaxTokens = Players.Count switch
        {
            2 => 6,
            3 => 5,
            4 => 4,
            5 => 3,
            6 => 3,
            _ => throw new InvalidOperationException("Invalid number of players for game configuration.")
        };
    }

    /// <summary>
    /// Resets the game for a new round
    /// </summary>
    public void StartNewRound()
    {
        List<Player> lastRoundWinner;
        try
        {
            lastRoundWinner = GetRoundWinners().ToList();
        }
        catch (GameNotStartedException)
        {
            lastRoundWinner = [];
        }

        // Clear all players' cards and reset protection
        foreach (var player in Players)
        {
            player.ResetForNewRound();
        }

        // Reset deck and shuffle
        InitializeDeck();
        ShuffleDeck();

        // Set aside the reserved card
        ReservedCard = DrawCard();

        // Deal initial cards to players
        foreach (var player in Players)
        {
            player.HandCard(DrawCard());
        }

        // Reset current player index
        var winner = lastRoundWinner.Count > 0 ?
            lastRoundWinner[new Random().Next(lastRoundWinner.Count)] : null;
        CurrentPlayerIndex = winner != null ? Players.IndexOf(winner) : 0;

        // Set game state to WaitingForDraw at the beginning of a round
        TransitionToState(GameStateEnum.WaitingForDraw);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates if a state transition is allowed and performs the transition
    /// </summary>
    public void TransitionToState(GameStateEnum newState)
    {
        if (CanTransitionToState(newState) == false)
        {
            throw new InvalidOperationException($"Invalid state transition from {GameState} to {newState}");
        }

        GameState = newState;
    }

    /// <summary>
    /// Checks if a state transition is valid according to the game rules
    /// </summary>
    public bool CanTransitionToState(GameStateEnum newState)
    {
        if (GameState == newState)
            return true;

        return GameState switch
        {
            GameStateEnum.WaitingForPlayers => newState == GameStateEnum.WaitingForDraw,
            GameStateEnum.WaitingForDraw => newState == GameStateEnum.WaitingForPlay || newState == GameStateEnum.Finished,
            GameStateEnum.WaitingForPlay => newState == GameStateEnum.WaitingForDraw || newState == GameStateEnum.Finished,
            GameStateEnum.Finished => false,
            _ => false,
        };
    }

    /// <summary>
    /// Returns a card to the deck
    /// </summary>
    public bool ReturnCardToDeck(CardType cardType)
    {
        CardsDeck = CardsDeck.Append(cardType).ToList();
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
}
