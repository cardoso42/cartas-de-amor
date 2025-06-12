using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;
using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Application.Interfaces;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Application.Extensions;
using Microsoft.Extensions.Logging;

namespace CartasDeAmor.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRoomRepository _roomRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<GameService> _logger;

    public GameService(IGameRoomRepository roomRepository, IPlayerRepository playerRepository, ILogger<GameService> logger)
    {
        _roomRepository = roomRepository;
        _playerRepository = playerRepository;
        _logger = logger;
        _logger.LogInformation("GameService initialized");
    }

    private List<InitialGameStatusDto> GetGameStatusDtos(Game game)
    {
        List<InitialGameStatusDto> startGameDtos = [];

        // Create a personalized GameStatusDto for each player
        foreach (var player in game.Players)
        {
            var playersStatuses = new List<PlayerStatusDto>();
            var players = game.Players.ToList();
            players.RemoveAll(p => p.UserEmail == player.UserEmail); // Exclude the current player

            playersStatuses.AddRange(
                players.Select(p => new PlayerStatusDto
                {
                    UserEmail = p.UserEmail,
                    Username = p.Username,
                    IsProtected = p.Protected,
                    Score = p.Score,
                    CardsInHand = p.HoldingCards.Count
                })
            );

            // Add the current player's private information
            startGameDtos.Add(new InitialGameStatusDto
            {
                OtherPlayersPublicData = playersStatuses,
                YourCards = player.HoldingCards,
                AllPlayersInOrder = game.Players.Select(p => p.UserEmail).ToList(),
                FirstPlayerIndex = game.CurrentPlayerIndex + 1,
                IsProtected = player.Protected
            });
        }

        return startGameDtos;
    }

    public async Task<IList<InitialGameStatusDto>> StartGameAsync(Guid roomId, string hostEmail)
    {
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

        game.GameStarted = true;
        game.StartNewRound();

        await _roomRepository.UpdateAsync(game);

        return GetGameStatusDtos(game);
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

    public async Task<PlayerUpdateDto> GetPlayerStatusAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        var player = game.Players.FirstOrDefault(p => p.UserEmail == userEmail) ?? throw new InvalidOperationException("Player not found in the game");

        return player.ToPlayerUpdateDto();
    }

    public async Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");
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

    public async Task<PlayerUpdateDto> DrawCardAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

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

        return currentPlayer.ToPlayerUpdateDto();
    }

    public Task<CardActionRequirements[]> GetCardRequirementsAsync(CardType cardType)
    {
        var card = CardFactory.Create(cardType);
        return Task.FromResult(card.GetCardActionRequirements().ToArray());
    }

    public async Task<InitialGameStatusDto> PlayCardAsync(Guid roomId, string userEmail, CardType cardType)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");
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
        if (currentPlayer.HoldingCards.Contains(cardType))
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
        currentPlayer.HoldingCards.Remove(cardType);

        // Draw a new card if there are cards left in the deck
        if (game.CardsDeck.Any())
        {
            var newCard = game.CardsDeck.First();
            game.CardsDeck.Remove(newCard);
            currentPlayer.HoldingCards.Add(newCard);
        }

        // Move to next player
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % players.Count;

        // Update the game and player
        await _playerRepository.UpdateAsync(currentPlayer);
        await _roomRepository.UpdateAsync(game);

        // Return the new game status
        var playerStatuses = players.Select(p => new PlayerStatusDto
        {
            UserEmail = p.UserEmail,
            Username = p.Username,
            IsProtected = p.Protected,
            Score = p.Score,
            CardsInHand = p.HoldingCards.Count
        }).ToList();

        return new InitialGameStatusDto
        {
            OtherPlayersPublicData = playerStatuses,
            YourCards = currentPlayer.HoldingCards,
            FirstPlayerIndex = game.CurrentPlayerIndex,
        };
    }

    public async Task<InitialGameStatusDto> PlayCardWithInputAsync(Guid roomId, string userEmail, CardType cardType, object[] additionalInputs)
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
        if (currentPlayer.HoldingCards.Contains(cardType))
        {
            throw new InvalidOperationException("You don't have that card");
        }

        // Create the card instance and apply its effects with additional inputs
        var card = CardFactory.Create(cardType);

        // For now, we'll implement a basic version that just plays the card
        // In a full implementation, we would handle the specific card logic with the additional inputs
        currentPlayer.PlayedCards.Add(cardType);
        currentPlayer.HoldingCards = [];

        // Draw a new card if there are cards left in the deck
        if (game.CardsDeck.Any())
        {
            var newCard = game.CardsDeck.First();
            game.CardsDeck.Remove(newCard);
            currentPlayer.HoldingCards.Add(newCard);
        }

        // Move to next player
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % players.Count;

        // Update the game and player
        await _playerRepository.UpdateAsync(currentPlayer);
        await _roomRepository.UpdateAsync(game);

        // Return the new game status
        var playerStatuses = players.Select(p => new PlayerStatusDto
        {
            UserEmail = p.UserEmail,
            Username = p.Username,
            IsProtected = p.Protected,
            Score = p.Score,
            CardsInHand = p.HoldingCards.Count
        }).ToList();

        return new InitialGameStatusDto
        {
            OtherPlayersPublicData = playerStatuses,
            YourCards = currentPlayer.HoldingCards,
            FirstPlayerIndex = game.CurrentPlayerIndex,
        };
    }

    public async Task NextPlayerAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        if (!game.GameStarted)
        {
            throw new InvalidOperationException("Game has not started yet");
        }

        game.AdvanceToNextPlayer();

        await _roomRepository.UpdateAsync(game);
    }
}
