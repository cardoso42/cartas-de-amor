using Microsoft.AspNetCore.SignalR;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Application.Extensions;
using MediatR;

namespace CartasDeAmor.Presentation.Hubs;

[Authorize]
public class GameHub(
    ILogger<GameHub> logger, IGameRoomService gameRoomService,
    IGameService gameService, IAccountService accountService,
    IConnectionMappingService connectionMapping, IMediator mediator) : Hub
{
    private readonly ILogger<GameHub> _logger = logger;
    private readonly IGameRoomService _gameRoomService = gameRoomService;
    private readonly IGameService _gameService = gameService;
    private readonly IAccountService _accountService = accountService;
    private readonly IConnectionMappingService _connectionMapping = connectionMapping;
    private readonly IMediator _mediator = mediator;

    public async Task JoinRoom(Guid roomId, string? password)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _connectionMapping.AddConnection(userEmail, Context.ConnectionId, roomId);

        await _gameRoomService.AddUserToRoomAsync(roomId, userEmail, password);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        
        _logger.LogInformation("User {User} joined room {RoomId}", userEmail, roomId);
    }

    public async Task LeaveRoom(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _connectionMapping.RemoveConnection(userEmail, Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        await _gameRoomService.RemoveUserFromRoomAsync(roomId, userEmail);

        _logger.LogInformation("User {User} left room {RoomId}", userEmail, roomId);
    }

    public async Task DrawCard(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _logger.LogInformation("User {User} is drawing a card in room {RoomId}", userEmail, roomId);

        try
        {
            await _gameService.DrawCardAsync(roomId, userEmail);
            _logger.LogInformation("User {User} drew a card in room {RoomId}", userEmail, roomId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to draw card for user {User} in room {RoomId}", userEmail, roomId);
            await Clients.Caller.SendAsync("DrawCardError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error drawing card for user {User} in room {RoomId}", userEmail, roomId);
            throw new HubException("Failed to draw card");
        }
    }

    public async Task StartGame(Guid roomId)
    {
        // Verify if the user is authenticated
        _logger.LogInformation("StartGame called for room {RoomId}", roomId);
        var userEmail = _accountService.GetEmailFromToken(Context.User);

        try
        {
            // Start the game through the game service
            await _gameService.StartGameAsync(roomId, userEmail);

            _logger.LogInformation("Game started in room {RoomId} by host {HostEmail}", roomId, userEmail);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to start game in room {RoomId} by user {UserEmail}", roomId, userEmail);
            await Clients.Caller.SendAsync("GameStartError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting game in room {RoomId}", roomId);
            throw new HubException("Failed to start game");
        }
    }

    public async Task GetCardRequirements(Guid roomId, CardType cardType)
    {
        _logger.LogInformation("Card requirements requested for card type {CardType}", cardType);

        var userEmail = _accountService.GetEmailFromToken(Context.User);

        try
        {
            await _gameService.GetCardActionRequirementsAsync(roomId, userEmail, cardType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting card requirements for card type {CardType}", cardType);
            throw new HubException("Failed to get card requirements");
        }
    }

    private async Task AdvanceGame(Guid roomId)
    {
        if (await _gameService.IsRoundOverAsync(roomId))
        {
            await HandleRoundEnd(roomId);
            return;
        }

        var nextTurnPlayer = await _gameService.NextPlayerAsync(roomId);
        await _mediator.SendNextTurnAsync(roomId, nextTurnPlayer);
    }

    private async Task HandleRoundEnd(Guid roomId)
    {
        _logger.LogInformation("Round in room {RoomId} is over", roomId);

        await _gameService.FinishRoundAsync(roomId);

        if (await _gameService.IsGameOverAsync(roomId))
        {
            _logger.LogInformation("Game in room {RoomId} is over", roomId);
            await _gameService.FinishGameAsync(roomId);
            return;
        }

        await _gameService.StartNewRoundAsync(roomId);

        var nextTurnPlayer = await _gameService.GetPlayerTurnAsync(roomId);
        await _mediator.SendNextTurnAsync(roomId, nextTurnPlayer);
    }

    public async Task PlayCard(Guid roomId, CardPlayDto cardPlayDto)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _logger.LogInformation("User {User} is playing card {CardType} in room {RoomId}", userEmail, cardPlayDto.CardType, roomId);

        try
        {
            var result = await _gameService.PlayCardAsync(roomId, userEmail, cardPlayDto);
            _logger.LogInformation("User {User} played card {CardType} in room {RoomId}", userEmail, cardPlayDto.CardType, roomId);

            if (result.ShouldAdvanceTurn)
            {
                await AdvanceGame(roomId);
            }
        }
        catch (MandatoryCardPlayViolationException ex)
        {
            _logger.LogWarning(ex, "Mandatory card play violation for user {User} in room {RoomId}", userEmail, roomId);
            await Clients.Caller.SendAsync("MandatoryCardPlay", ex.Message, ex.RequiredCardType);
        }
        catch (CardRequirementsNotMetException ex)
        {
            _logger.LogWarning(ex, "Card requirements not met for user {User} in room {RoomId}. Resending them", userEmail, roomId);
            await _gameService.GetCardActionRequirementsAsync(roomId, userEmail, cardPlayDto.CardType);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to play card for user {User} in room {RoomId}", userEmail, roomId);
            await Clients.Caller.SendAsync("PlayCardError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing card for user {User} in room {RoomId}", userEmail, roomId);
            throw new HubException("Failed to play card");
        }
    }

    public async Task SubmitCardChoice(Guid roomId, CardType keepCardType, List<CardType> returnCardTypes)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _logger.LogInformation("User {User} is submitting card choice in room {RoomId}", userEmail, roomId);

        try
        {
            await _gameService.SubmitCardChoiceAsync(roomId, userEmail, keepCardType, returnCardTypes);
            await AdvanceGame(roomId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to submit card choice for user {User} in room {RoomId}", userEmail, roomId);
            await Clients.Caller.SendAsync("CardChoiceError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting card choice for user {User} in room {RoomId}", userEmail, roomId);
            throw new HubException("Failed to submit card choice");
        }
    }

    public async Task ReconnectToRoom(Guid roomId)
    {
        try
        {
            var userEmail = _accountService.GetEmailFromToken(Context.User);

            _connectionMapping.AddConnection(userEmail, Context.ConnectionId, roomId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconnecting to room");
            throw new HubException("Failed to reconnect to room");
        }
    }

    public async Task GetCurrentGameStatus(Guid roomId)
    {
        try
        {
            var userEmail = _accountService.GetEmailFromToken(Context.User);

            // Check if there's an active game and send the current status
            var currentGameStatus = await _gameService.GetCurrentGameStatusAsync(roomId, userEmail);
            if (currentGameStatus != null)
            {
                // Send the current game status to the requesting player
                // The game status already contains FirstPlayerIndex with current turn info
                await Clients.Caller.SendAsync("CurrentGameStatus", currentGameStatus);
            }
            else
            {
                // No active game found, send empty status
                await Clients.Caller.SendAsync("CurrentGameStatus", null);
            }

            _logger.LogInformation("User {User} requested current game status for room {RoomId}", userEmail, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current game status");
            throw new HubException("Failed to get current game status");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove connection from mapping when user disconnects
        var roomId = _connectionMapping.GetRoomIdByConnectionId(Context.ConnectionId);
        _connectionMapping.RemoveConnectionById(Context.ConnectionId);
        
        if (roomId.HasValue)
        {
            await _gameService.VerifyGameValidity(roomId.Value);
        }
        
        // SignalR will automatically remove the connection from all groups
        await base.OnDisconnectedAsync(exception);
    }
}
