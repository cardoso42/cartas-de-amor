using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;
using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Application.Interfaces;
using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRoomRepository _roomRepository;
    private readonly IPlayerRepository _playerRepository;

    public GameService(IGameRoomRepository roomRepository, IPlayerRepository playerRepository)
    {
        _roomRepository = roomRepository;
        _playerRepository = playerRepository;
    }

    public async Task<IList<GameStatusDto>> StartGameAsync(Guid roomId, string hostEmail)
    {
        List<GameStatusDto> startGameDtos = [];

        // Get the game room
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        // Verify the user is the host
        if (game.HostEmail != hostEmail)
        {
            throw new InvalidOperationException("Only the host can start the game");
        }

        // Check if we have enough players (Love Letter requires 2-4 players)
        if (game.Players.Count < 2)
        {
            throw new InvalidOperationException("At least 2 players are required to start the game");
        }

        var playersNames = game.Players.Select(p => p.UserEmail).ToList();

        // Create and shuffle the deck
        var deck = CardFactory.CreateShuffledDeck().Select(c => c.CardType).ToList();

        // Reserve one card for the end of the round
        game.ReservedCard = deck[0];
        deck.RemoveAt(0);

        // Deal one card to each player
        var players = game.Players.ToList();

        var playerStatuses = new List<PlayerStatus>();
        for (int i = 0; i < players.Count; i++)
        {
            players[i].HoldingCard = deck[i];
            players[i].PlayedCards = [];
            players[i].Protected = false;
            await _playerRepository.UpdateAsync(players[i]);

            playerStatuses.Add(new PlayerStatus
            {
                UserEmail = players[i].UserEmail,
                Username = players[i].Username,
                IsProtected = players[i].Protected,
                Score = players[i].Score,
                CardsInHand = 1 // Each player starts with one card
            });
        }

        for (int i = 0; i < players.Count; i++)
        {
            startGameDtos.Add(new GameStatusDto
            {
                Players = playerStatuses,
                YourCard = players[i].HoldingCard ?? default,
                FirstPlayerIndex = 0,
            });
        }

        // Remove dealt cards from deck
        for (int i = 0; i < players.Count; i++)
        {
            deck.RemoveAt(0);
        }

        // Store remaining deck in the game
        game.CardsDeck = deck;

        // Set game as started and set current player index
        game.GameStarted = true;
        game.CurrentPlayerIndex = 0;

        // Update the game
        await _roomRepository.UpdateAsync(game);

        return startGameDtos;
    }

    public async Task<IList<Player>> GetPlayersAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        return [.. game.Players];
    }

    public async Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        if (!game.GameStarted)
        {
            return false;
        }

        var players = game.Players.ToList();
        if (game.CurrentPlayerIndex >= players.Count)
        {
            return false;
        }

        return players[game.CurrentPlayerIndex].UserEmail == userEmail;
    }

    public Task<CardActionRequirements[]> GetCardRequirementsAsync(CardType cardType)
    {
        var card = CardFactory.Create(cardType);
        return Task.FromResult(card.GetCardActionRequirements().ToArray());
    }

    public async Task<GameStatusDto> PlayCardAsync(Guid roomId, string userEmail, CardType cardType)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        if (!game.GameStarted)
        {
            throw new InvalidOperationException("Game has not started yet");
        }

        // Verify it's the player's turn
        if (!await IsPlayerTurnAsync(roomId, userEmail))
        {
            throw new InvalidOperationException("It's not your turn");
        }

        var players = game.Players.ToList();
        var currentPlayer = players[game.CurrentPlayerIndex];

        // Verify the player has the card they're trying to play
        if (currentPlayer.HoldingCard != cardType)
        {
            throw new InvalidOperationException("You don't have that card");
        }

        // Create the card instance to get its requirements
        var card = CardFactory.Create(cardType);
        var requirements = card.GetCardActionRequirements();

        // For cards that require additional input, we need to handle that separately
        if (requirements.Any(r => r != CardActionRequirements.None))
        {
            // This indicates the card needs more information
            throw new InvalidOperationException("Card requires additional input");
        }

        // Play the card (apply its effects)
        // For now, we'll just move the card to played cards and draw a new one
        currentPlayer.PlayedCards.Add(cardType);
        currentPlayer.HoldingCard = null;

        // Draw a new card if there are cards left in the deck
        if (game.CardsDeck.Any())
        {
            var newCard = game.CardsDeck.First();
            game.CardsDeck.Remove(newCard);
            currentPlayer.HoldingCard = newCard;
        }

        // Move to next player
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % players.Count;

        // Update the game and player
        await _playerRepository.UpdateAsync(currentPlayer);
        await _roomRepository.UpdateAsync(game);

        // Return the new game status
        var playerStatuses = players.Select(p => new PlayerStatus
        {
            UserEmail = p.UserEmail,
            Username = p.Username,
            IsProtected = p.Protected,
            Score = p.Score,
            CardsInHand = p.HoldingCard.HasValue ? 1 : 0
        }).ToList();

        return new GameStatusDto
        {
            Players = playerStatuses,
            YourCard = currentPlayer.HoldingCard ?? default,
            FirstPlayerIndex = game.CurrentPlayerIndex,
        };
    }

    public async Task<GameStatusDto> PlayCardWithInputAsync(Guid roomId, string userEmail, CardType cardType, object[] additionalInputs)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        if (!game.GameStarted)
        {
            throw new InvalidOperationException("Game has not started yet");
        }

        // Verify it's the player's turn
        if (!await IsPlayerTurnAsync(roomId, userEmail))
        {
            throw new InvalidOperationException("It's not your turn");
        }

        var players = game.Players.ToList();
        var currentPlayer = players[game.CurrentPlayerIndex];

        // Verify the player has the card they're trying to play
        if (currentPlayer.HoldingCard != cardType)
        {
            throw new InvalidOperationException("You don't have that card");
        }

        // Create the card instance and apply its effects with additional inputs
        var card = CardFactory.Create(cardType);
        
        // For now, we'll implement a basic version that just plays the card
        // In a full implementation, we would handle the specific card logic with the additional inputs
        currentPlayer.PlayedCards.Add(cardType);
        currentPlayer.HoldingCard = null;

        // Draw a new card if there are cards left in the deck
        if (game.CardsDeck.Any())
        {
            var newCard = game.CardsDeck.First();
            game.CardsDeck.Remove(newCard);
            currentPlayer.HoldingCard = newCard;
        }

        // Move to next player
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % players.Count;

        // Update the game and player
        await _playerRepository.UpdateAsync(currentPlayer);
        await _roomRepository.UpdateAsync(game);

        // Return the new game status
        var playerStatuses = players.Select(p => new PlayerStatus
        {
            UserEmail = p.UserEmail,
            Username = p.Username,
            IsProtected = p.Protected,
            Score = p.Score,
            CardsInHand = p.HoldingCard.HasValue ? 1 : 0
        }).ToList();

        return new GameStatusDto
        {
            Players = playerStatuses,
            YourCard = currentPlayer.HoldingCard ?? default,
            FirstPlayerIndex = game.CurrentPlayerIndex,
        };
    }
}
