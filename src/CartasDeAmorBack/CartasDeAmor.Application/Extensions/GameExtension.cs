using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.Extensions;

public static class GameExtension
{
    /// <summary>
    /// Gets the current player in the game
    /// </summary>
    public static Player? GetCurrentPlayer(this Game game)
    {
        if (game.Players.Count == 0 || game.CurrentPlayerIndex >= game.Players.Count)
            return null;
            
        return game.Players.ElementAt(game.CurrentPlayerIndex);
    }

    /// <summary>
    /// Gets the next player in turn order
    /// </summary>
    public static Player? GetNextPlayer(this Game game)
    {
        if (game.Players.Count == 0)
            return null;
            
        var nextIndex = (game.CurrentPlayerIndex + 1) % game.Players.Count;
        return game.Players.ElementAt(nextIndex);
    }

    /// <summary>
    /// Advances to the next player's turn
    /// </summary>
    public static void AdvanceToNextPlayer(this Game game)
    {       
        if (game.Players.Count > 0)
        {
            game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % game.Players.Count;
            var nextPlayer = game.Players.ElementAt(game.CurrentPlayerIndex);

            game.HandCardToPlayer(nextPlayer.UserEmail);

            game.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Checks if the game is full (reached maximum players)
    /// </summary>
    public static bool IsFull(this Game game, int maxPlayers = 4)
    {
        return game.Players.Count >= maxPlayers;
    }

    /// <summary>
    /// Gets a player by their email
    /// </summary>
    public static Player? GetPlayerByEmail(this Game game, string email)
    {
        return game.Players.FirstOrDefault(p => p.UserEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a player by their username
    /// </summary>
    public static Player? GetPlayerByUsername(this Game game, string username)
    {
        return game.Players.FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a player with the given email exists in the game
    /// </summary>
    public static bool HasPlayer(this Game game, string email)
    {
        return game.GetPlayerByEmail(email) != null;
    }

    /// <summary>
    /// Checks if the game can be started (minimum players requirement)
    /// </summary>
    public static bool CanStart(this Game game, int minPlayers = 2)
    {
        return !game.GameStarted && game.Players.Count >= minPlayers;
    }

    /// <summary>
    /// Gets the number of active (not eliminated) players
    /// </summary>
    public static int GetActivePlayerCount(this Game game)
    {
        return game.Players.Count(p => p.HoldingCards.Count > 0);
    }

    /// <summary>
    /// Gets all active (not eliminated) players
    /// </summary>
    public static IEnumerable<Player> GetActivePlayers(this Game game)
    {
        return game.Players.Where(p => p.HoldingCards.Count > 0);
    }

    /// <summary>
    /// Gets all eliminated players
    /// </summary>
    public static IEnumerable<Player> GetEliminatedPlayers(this Game game)
    {
        return game.Players.Where(p => p.HoldingCards.Count == 0);
    }

    /// <summary>
    /// Checks if the game round is over (only one player remaining or deck is empty)
    /// </summary>
    public static bool IsRoundOver(this Game game)
    {
        return game.GetActivePlayerCount() <= 1 || game.CardsDeck.Count == 0;
    }

    /// <summary>
    /// Gets the winner of the current round
    /// </summary>
    public static Player? GetRoundWinner(this Game game)
    {
        var activePlayers = game.GetActivePlayers().ToList();
        
        if (activePlayers.Count == 1)
            return activePlayers.First();
            
        if (activePlayers.Count == 0)
            return null;

        // If multiple players remain, highest card wins
        var maxCardValue = activePlayers.Max(p => (int)p.HoldingCards.Max(c => c));
        return activePlayers.FirstOrDefault(p => (int)p.HoldingCards.Max(c => c) == maxCardValue);
    }

    /// <summary>
    /// Draws a card from the deck
    /// </summary>
    public static CardType? DrawCard(this Game game)
    {
        if (game.CardsDeck.Count == 0)
            return null;

        var card = game.CardsDeck.First();
        game.CardsDeck.Remove(card);
        game.UpdatedAt = DateTime.UtcNow;
        
        return card;
    }

    /// <summary>
    /// Shuffles the deck
    /// </summary>
    public static void ShuffleDeck(this Game game)
    {
        var random = new Random();
        var shuffledCards = game.CardsDeck.OrderBy(x => random.Next()).ToList();
        game.CardsDeck.Clear();
        
        foreach (var card in shuffledCards)
        {
            game.CardsDeck.Add(card);
        }
        
        game.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new game deck with the standard Love Letter card distribution
    /// </summary>
    public static void InitializeDeck(this Game game)
    {
        game.CardsDeck.Clear();
        
        // Standard Love Letter deck composition
        game.CardsDeck.Add(CardType.Spy);         // 2 cards
        game.CardsDeck.Add(CardType.Spy);
        
        game.CardsDeck.Add(CardType.Guard);       // 5 cards
        game.CardsDeck.Add(CardType.Guard);
        game.CardsDeck.Add(CardType.Guard);
        game.CardsDeck.Add(CardType.Guard);
        game.CardsDeck.Add(CardType.Guard);
        
        game.CardsDeck.Add(CardType.Priest);      // 2 cards
        game.CardsDeck.Add(CardType.Priest);
        
        game.CardsDeck.Add(CardType.Baron);       // 2 cards
        game.CardsDeck.Add(CardType.Baron);
        
        game.CardsDeck.Add(CardType.Servant);     // 2 cards
        game.CardsDeck.Add(CardType.Servant);
        
        game.CardsDeck.Add(CardType.Prince);      // 2 cards
        game.CardsDeck.Add(CardType.Prince);
        
        game.CardsDeck.Add(CardType.Chanceller);  // 2 cards
        game.CardsDeck.Add(CardType.Chanceller);

        game.CardsDeck.Add(CardType.King);        // 1 card
        game.CardsDeck.Add(CardType.Countess);    // 1 card
        game.CardsDeck.Add(CardType.Princess);    // 1 card
        
        game.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the game is over (a player reached the target score)
    /// </summary>
    public static bool IsGameOver(this Game game, int targetScore = 4)
    {
        return game.Players.Any(p => p.Score >= targetScore);
    }

    /// <summary>
    /// Gets the overall winner of the game
    /// </summary>
    public static Player? GetGameWinner(this Game game, int targetScore = 4)
    {
        return game.Players.FirstOrDefault(p => p.Score >= targetScore);
    }

    public static Player HandCardToPlayer(this Game game, string playerEmail)
    {
        var player = game.GetPlayerByEmail(playerEmail);
        if (player == null)
            throw new InvalidOperationException($"Player with email {playerEmail} not found in the game.");

        var card = game.DrawCard() ?? throw new InvalidOperationException("No cards left in the deck to draw.");
        player.HandCard(card);

        return player;
    }

    /// <summary>
    /// Resets the game for a new round
    /// </summary>
    public static void StartNewRound(this Game game)
    {
        // Clear all players' cards and reset protection
        foreach (var player in game.Players)
        {
            player.HoldingCards = [];
            player.Protected = false;
            player.PlayedCards.Clear();
        }

        // Reset deck and shuffle
        game.InitializeDeck();
        game.ShuffleDeck();

        // Set aside the reserved card
        game.ReservedCard = game.DrawCard() ?? throw new InvalidOperationException("Failed to draw reserved card");

        // Deal initial cards to players
        foreach (var player in game.Players)
        {
            player.HandCard(game.DrawCard() ?? throw new InvalidOperationException("Failed to draw initial card for player"));
        }

        // Reset current player index
        game.CurrentPlayerIndex = 0;
        game.UpdatedAt = DateTime.UtcNow;
    }
}