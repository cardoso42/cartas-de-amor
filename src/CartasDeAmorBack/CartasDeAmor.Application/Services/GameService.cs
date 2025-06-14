using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;
using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Application.Interfaces;
using CartasDeAmor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CartasDeAmor.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRoomRepository _roomRepository;
    private readonly ICardService _cardService;
    private readonly ILogger<GameService> _logger;

    public GameService(IGameRoomRepository roomRepository, ICardService cardService, ILogger<GameService> logger)
    {
        _roomRepository = roomRepository;
        _cardService = cardService;
        _logger = logger;
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

        return new PlayerUpdateDto(player);
    }

    public async Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (!game.GameStarted)
        {
            _logger.LogWarning("Game has not started yet for room {RoomId}", roomId);
            return false;
        }

        var players = game.Players.ToList();
        if (game.CurrentPlayerIndex >= players.Count)
        {
            _logger.LogWarning("Current player index {CurrentPlayerIndex} is out of bounds for players count {PlayersCount} in room {RoomId}",
                game.CurrentPlayerIndex, players.Count, roomId);
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

        // We can only draw a card when waiting for draw
        if (game.GameState != GameStateEnum.WaitingForDraw)
        {
            throw new InvalidOperationException($"Cannot draw a card in current state: {game.GameState}");
        }

        var players = game.Players.ToList();
        var currentPlayer = players[game.CurrentPlayerIndex];

        game.HandCardToPlayer(currentPlayer.UserEmail);
        game.TransitionToState(GameStateEnum.WaitingForPlay);

        await _roomRepository.UpdateAsync(game);

        return new PlayerUpdateDto(currentPlayer);
    }

    public async Task NextPlayerAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        if (!game.GameStarted)
        {
            throw new InvalidOperationException("Game has not started yet");
        }

        game.AdvanceToNextPlayer();
        game.TransitionToState(GameStateEnum.WaitingForDraw);

        await _roomRepository.UpdateAsync(game);
    }

    public async Task<CardActionResultDto> PlayCardAsync(Guid roomId, string userEmail, CardType cardType)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (!game.GameStarted)
        {
            _logger.LogWarning("Game has not started yet for room {RoomId}", roomId);
            throw new InvalidOperationException("Game has not started yet");
        }

        // Verify it's the player's turn
        if (!await IsPlayerTurnAsync(roomId, userEmail))
        {
            _logger.LogWarning("It's not the player's turn for user {UserEmail} in room {RoomId}", userEmail, roomId);
            throw new InvalidOperationException("It's not your turn");
        }

        // We can only play a card when waiting for play
        if (game.GameState != GameStateEnum.WaitingForPlay)
        {
            _logger.LogWarning("Cannot play a card in current state {GameState} for room {RoomId}", game.GameState, roomId);
            throw new InvalidOperationException($"Cannot play a card in current state: {game.GameState}");
        }

        var players = game.Players.ToList();
        var currentPlayer = players[game.CurrentPlayerIndex];
        if (!currentPlayer.HoldingCards.Contains(cardType))
        {
            _logger.LogWarning("Player {UserEmail} does not have card {CardType} in hand for room {RoomId}", userEmail, cardType, roomId);
            throw new InvalidOperationException("You do not have this card in your hand");
        }

        // Play the card
        _logger.LogInformation("Player {UserEmail} is playing card {CardType} in room {RoomId}", userEmail, cardType, roomId);
        game.PlayCard(currentPlayer.UserEmail, cardType);

        return new CardActionResultDto
        {
            
            Success = true,
        };
    }

    public async Task<CardRequirementsDto> GetCardActionRequirements(Guid roomId, string userEmail, CardType cardType)
    {
        var requirements = await _cardService.GetCardActionRequirementsAsync(roomId, userEmail, cardType);

        _logger.LogInformation("Card {CardType} action requirements: {Requirements}", cardType, requirements);

        return requirements;
    }
}
