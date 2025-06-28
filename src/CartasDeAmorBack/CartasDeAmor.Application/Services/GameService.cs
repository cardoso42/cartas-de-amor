using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;
using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Application.Interfaces;
using CartasDeAmor.Domain.Entities;
using Microsoft.Extensions.Logging;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Configuration;
using CartasDeAmor.Application.Extensions;
using CartasDeAmor.Domain.Events;
using MediatR;

namespace CartasDeAmor.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GameService> _logger;
    private readonly IMediator _mediator;

    public GameService(
        IGameRoomRepository roomRepository, IUserRepository userRepository,
        ILogger<GameService> logger, IMediator mediator)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _logger = logger;
        _mediator = mediator;
    }

    private async Task SendGameStatusDtos(Game game)
    {
        // Get all player emails
        var playerEmails = game.Players.Select(p => p.UserEmail).ToList();
        
        // Fetch all users in a single query
        var users = (await _userRepository.GetByEmailsAsync(playerEmails))
            .ToDictionary(u => u.Email, u => u);

        foreach (var player in game.Players)
        {
            // Get other players' data for this specific player
            var otherPlayers = game.Players
                .Where(p => p.UserEmail != player.UserEmail)
                .Select(p => new PlayerStatusDto 
                {
                    UserEmail = p.UserEmail, 
                    Username = users.ContainsKey(p.UserEmail) ? users[p.UserEmail].Username : p.UserEmail, // Use actual username if available, fallback to email
                    Status = p.Status,
                    IsProtected = p.IsProtected(),
                    Score = p.Score,
                    CardsInHand = p.HoldingCards.Count
                })
                .ToList();

            // Send round start data to the specific player (this is the main message the frontend expects)
            await _mediator.SendRoundStartAsync(player.UserEmail, new InitialGameStatusDto(game, otherPlayers, player));
        }
    }

    public async Task StartGameAsync(Guid roomId, string hostEmail)
    {
        // Get the game room
        var game = await _roomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException("Room not found");

        // Verify the user is the host
        if (game.HostEmail != hostEmail)
        {
            throw new InvalidOperationException("Only the host can start the game");
        }

        // Check if we have enough players and not too many
        if (game.Players.Count < 2)
        {
            throw new InvalidOperationException("At least 2 players are required to start the game");
        }

        if (game.Players.Count > GameSettings.MaxPlayers)
        {
            throw new InvalidOperationException($"A maximum of {GameSettings.MaxPlayers} players are allowed in a game");
        }

        game.ConfigureGame();
        game.StartNewRound();
        game.TransitionToState(GameStateEnum.WaitingForDraw);

        await _roomRepository.UpdateAsync(game);

        await SendGameStatusDtos(game);
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

    public async Task<PrivatePlayerUpdateDto> GetPlayerStatusAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        var player = game.Players.FirstOrDefault(p => p.UserEmail == userEmail) ?? throw new InvalidOperationException("Player not found in the game");

        return new PrivatePlayerUpdateDto(player);
    }

    public async Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (game.HasStarted() == false)
        {
            _logger.LogWarning("Game has not started yet for room {RoomId}", roomId);
            throw new GameNotStartedException("Game has not started yet");
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

    public async Task DrawCardAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        if (game.HasStarted() == false)
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

        // Send player drew card event to all players in room
        await _mediator.SendGameEventAsync(roomId, null, "PlayerDrewCard", currentPlayer.UserEmail);
        
        // Send player updates
        await _mediator.SendPlayerUpdatePublicAsync(roomId, currentPlayer);
        await _mediator.SendPlayerUpdatePrivateAsync(currentPlayer);
    }

    public async Task<string> NextPlayerAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        if (game.HasStarted() == false)
        {
            throw new InvalidOperationException("Game has not started yet");
        }

        game.AdvanceToNextPlayer();
        game.TransitionToState(GameStateEnum.WaitingForDraw);

        var newPlayer = game.GetCurrentPlayer() ?? throw new InvalidOperationException("Current player not found");
        newPlayer.SetProtection(false); // Remove protection at the start of a new turn

        await _roomRepository.UpdateAsync(game);

        return newPlayer.UserEmail;
    }

    public async Task<CardResult> PlayCardAsync(Guid roomId, string userEmail, CardPlayDto cardPlay)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (game.HasStarted() == false)
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

        // Player must have the card in hand
        var currentPlayer = game.GetPlayerByEmail(userEmail) ?? throw new InvalidOperationException("Player not found in the game");
        if (!currentPlayer.HoldingCards.Contains(cardPlay.CardType))
        {
            _logger.LogWarning("Player {UserEmail} does not have card {CardType} in hand for room {RoomId}", userEmail, cardPlay.CardType, roomId);
            throw new InvalidOperationException("You do not have this card in your hand");
        }

        // Check if another card should be played instead
        var playerCards = currentPlayer.GetHandCopy();
        foreach (var playerCard in playerCards)
        {
            if (playerCard == cardPlay.CardType) continue;

            var cardInstance = CardFactory.Create(playerCard);
            if (cardInstance.MustBePlayed(currentPlayer))
            {
                _logger.LogWarning("Player {UserEmail} must play card {CardType} instead of {PlayedCardType} in room {RoomId}",
                    userEmail, playerCard, cardPlay.CardType, roomId);
                throw new MandatoryCardPlayViolationException($"You must play card {playerCard} instead", cardPlay.CardType, playerCard);
            }
        }

        // Play the card
        _logger.LogInformation("Player {UserEmail} is playing card {CardType} in room {RoomId}", userEmail, cardPlay.CardType, roomId);

        var card = CardFactory.Create(cardPlay.CardType);
        var targetPlayer = game.GetPlayerByEmail(cardPlay.TargetPlayerEmail ?? string.Empty);

        var result = new CardResult();
        try
        {
            currentPlayer.PlayCard(cardPlay.CardType);
            result = card.Play(game, currentPlayer, targetPlayer, cardPlay.TargetCardType);
        }
        catch (PlayerProtectedException ex)
        {
            // Player targeted protected player, player wasted their turn
            _logger.LogWarning("Player {UserEmail} cannot target player {TargetPlayerEmail} due to protection in room {RoomId}",
                userEmail, ex.PlayerEmail, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing card {CardType} for player {UserEmail} in room {RoomId}", cardPlay.CardType, userEmail, roomId);
            currentPlayer.RevertPlayCard(cardPlay.CardType);
            throw;
        }

        await _roomRepository.UpdateAsync(game);

        // Send player updates via MediatR
        await _mediator.SendPlayerUpdatePublicAsync(roomId, currentPlayer);
        await _mediator.SendPlayerUpdatePrivateAsync(currentPlayer);

        if (targetPlayer != null)
        {
            await _mediator.SendPlayerUpdatePublicAsync(roomId, targetPlayer);
            await _mediator.SendPlayerUpdatePrivateAsync(targetPlayer);
        }

        // Process game events from card play
        await ProcessGameEvents(roomId, result.Events);

        return result;
    }

    public async Task GetCardActionRequirementsAsync(Guid roomId, string currentPlayer, CardType cardType)
    {
        var gameRoom = await _roomRepository.GetByIdAsync(roomId) ?? throw new ArgumentException("Game room not found.");
        var card = CardFactory.Create(cardType);

        var requirements = card.GetCardActionRequirements();

        if (requirements == null)
        {
            await _mediator.SendCardRequirementsAsync(
                currentPlayer, new CardRequirementsDto { CardType = cardType });
            return;
        }

        var requirementsDto = new CardRequirementsDto { CardType = cardType };

        if (requirements.IsTargetRequired)
        {
            requirementsDto.Requirements.Add(CardActionRequirements.SelectPlayer);
            requirementsDto.PossibleTargets = gameRoom.Players
                .Where(p => p.IsInGame())
                .Select(p => p.UserEmail)
                .ToList();

            if (!requirements.CanChooseSelf)
            {
                requirementsDto.PossibleTargets.Remove(currentPlayer);
            }
        }

        if (requirements.IsCardTypeRequired)
        {
            requirementsDto.Requirements.Add(CardActionRequirements.SelectCardType);
            requirementsDto.PossibleCardTypes = Enum.GetValues<CardType>().ToList();

            if (!requirements.CanChooseEqualCardType)
            {
                requirementsDto.PossibleCardTypes.Remove(cardType);
            }
        }

        await _mediator.SendCardRequirementsAsync(currentPlayer, requirementsDto);
    }

    public async Task<bool> IsRoundOverAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException("Room not found");

        return game.IsRoundOver();
    }

    public async Task<bool> IsGameOverAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException("Room not found");

        return game.IsGameOver();
    }

    public async Task FinishRoundAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException("Room not found");

        if (!game.IsRoundOver())
        {
            throw new InvalidOperationException("Cannot finish round while it is still ongoing");
        }

        // Get game winners (usually just one, but may be more)
        var winners = game.GetRoundWinners();
        foreach (var winner in winners)
        {
            winner.AddScore(1);
        }

        // Get users with bonus points from cards that have bonus point conditions
        var bonusReceivers = new List<string>();
        foreach (var cardType in Enum.GetValues<CardType>())
        {
            var card = CardFactory.Create(cardType);
            if (card.ConditionForExtraPoint == null) continue;

            foreach (var player in game.Players)
            {
                if (card.ConditionForExtraPoint(game, player))
                {
                    player.AddScore(1);
                    bonusReceivers.Add(player.UserEmail);
                }
            }
        }

        await _roomRepository.UpdateAsync(game);

        // Send round winners event to all players in room
        await _mediator.SendGameEventAsync(roomId, null, "RoundWinners", winners.Select(w => w.UserEmail).ToList());
        
        // Send bonus points event to all players in room
        await _mediator.SendGameEventAsync(roomId, null, "BonusPoints", bonusReceivers);
    }

    public async Task FinishGameAsync(Guid roomdId)
    {
        var game = await _roomRepository.GetByIdAsync(roomdId)
            ?? throw new InvalidOperationException("Room not found");

        game.TransitionToState(GameStateEnum.Finished);
        await _roomRepository.UpdateAsync(game);

        // Send game over event to all players in room
        await _mediator.SendGameEventAsync(roomdId, null, "GameOver", game.GetGameWinner().Select(p => p.UserEmail).ToList());
    }

    public async Task<string> GetPlayerTurnAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException("Room not found");

        if (game.HasStarted() == false)
        {
            throw new InvalidOperationException("Game has not started yet");
        }

        if (game.CurrentPlayerIndex >= game.Players.Count)
        {
            throw new InvalidOperationException("Current player index is out of bounds");
        }

        return game.Players[game.CurrentPlayerIndex].UserEmail;
    }

    public async Task StartNewRoundAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException("Room not found");

        if (game.HasStarted() == false)
        {
            throw new InvalidOperationException("Game has not started yet");
        }

        if (!game.IsRoundOver())
        {
            throw new InvalidOperationException("Cannot start a new round while the current round is still ongoing");
        }

        game.StartNewRound();
        await _roomRepository.UpdateAsync(game);

        await SendGameStatusDtos(game);
    }

    public async Task SubmitCardChoiceAsync(Guid roomId, string userEmail, CardType keepCardType, List<CardType> returnCardsType)
    {
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        if (game.HasStarted() == false)
        {
            throw new InvalidOperationException("Game has not started yet");
        }

        // Verify it's the player's turn
        if (!await IsPlayerTurnAsync(roomId, userEmail))
        {
            throw new InvalidOperationException("It's not your turn");
        }

        // We can only submit card choices when waiting for card play
        if (game.GameState != GameStateEnum.WaitingForPlay)
        {
            throw new InvalidOperationException($"Cannot submit card choice in current state: {game.GameState}");
        }

        var player = game.GetPlayerByEmail(userEmail) ?? throw new InvalidOperationException("Player not found in the game");
        if (!player.HoldingCards.Contains(keepCardType))
        {
            throw new InvalidOperationException("You do not have the card you are trying to keep");
        }
        if (returnCardsType.Any(card => !player.HoldingCards.Contains(card)))
        {
            throw new InvalidOperationException("You do not have all the cards you are trying to return");
        }

        foreach (var cardType in returnCardsType)
        {
            player.RemoveCard(cardType);
            game.ReturnCardToDeck(cardType);
        }

        await _roomRepository.UpdateAsync(game);

        _logger.LogInformation("Player {UserEmail} has submitted card choice: keep {KeepCardType}, return {ReturnCardsType} in room {RoomId}",
            userEmail, keepCardType, string.Join(", ", returnCardsType), roomId);

        // Send player updates via MediatR
        await _mediator.SendPlayerUpdatePublicAsync(roomId, player);
        
        if (returnCardsType.Count > 0)
        {
            await _mediator.SendGameEventAsync(roomId, null, "CardReturnedToDeck", new
            {
                Player = userEmail,
                CardCount = returnCardsType.Count
            });
        }

        await _mediator.SendPlayerUpdatePrivateAsync(player);
    }

    public async Task<InitialGameStatusDto?> GetCurrentGameStatusAsync(Guid roomId, string userEmail)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            return null;
        }

        // Check if the user is a player in this game
        var player = game.Players.FirstOrDefault(p => p.UserEmail == userEmail);
        if (player == null)
        {
            return null;
        }

        // Only return game status if the game has started
        if (!game.HasStarted())
        {
            return null;
        }

        var otherPlayers = game.Players
            .Where(p => p.UserEmail != userEmail)
            .ToList();

        // Get all user emails for other players
        var otherPlayerEmails = otherPlayers.Select(p => p.UserEmail).ToList();
        
        // Fetch all users in a single query to avoid N+1 problem
        var users = (await _userRepository.GetByEmailsAsync(otherPlayerEmails))
            .ToDictionary(u => u.Email, u => u);

        // Log warning if some users are missing
        var missingUsers = otherPlayerEmails.Except(users.Keys).ToList();
        if (missingUsers.Any())
        {
            _logger.LogWarning("Some users not found in database for game {RoomId}: {MissingUsers}", 
                roomId, string.Join(", ", missingUsers));
        }

        // Create PlayerStatusDto objects for players that have corresponding users
        var otherPlayersPublicData = otherPlayers
            .Where(player => users.ContainsKey(player.UserEmail))
            .Select(player => new PlayerStatusDto(users[player.UserEmail], player))
            .ToList();

        return new InitialGameStatusDto(game, otherPlayersPublicData, player);
    }

    public async Task VerifyGameValidity(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException("Room not found");

        var players = game.Players;
        if (players.Count <= 1)
        {
            await FinishGameAsync(roomId);
        }
    }

    /// <summary>
    /// Processes a list of game events and converts them to MediatR commands
    /// </summary>
    private async Task ProcessGameEvents(Guid roomId, List<GameEvent> events)
    {
        foreach (var gameEvent in events)
        {
            await _mediator.SendGameEventAsync(
                roomId,
                gameEvent.Destination,
                gameEvent.EventType,
                gameEvent.EventData);
        }
    }
}
