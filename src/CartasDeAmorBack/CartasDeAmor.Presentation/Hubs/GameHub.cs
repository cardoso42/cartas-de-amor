using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CartasDeAmor.Presentation.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;
    private readonly IGameRoomService _gameRoomService;
    private readonly IGameService _gameService;
    private readonly IAccountService _accountService;
    private readonly IConnectionMappingService _connectionMapping;

    public GameHub(ILogger<GameHub> logger, IGameRoomService gameRoomService, IGameService gameService, IAccountService accountService, IConnectionMappingService connectionMapping)
    {
        _logger = logger;
        _gameRoomService = gameRoomService;
        _gameService = gameService;
        _accountService = accountService;
        _connectionMapping = connectionMapping;
    }

    public async Task JoinRoom(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);
        _connectionMapping.AddConnection(userEmail, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        await _gameRoomService.AddUserToRoomAsync(roomId, userEmail);
        
        _logger.LogInformation("User {User} joined room {RoomId}", userEmail, roomId);
    }

    public async Task LeaveRoom(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);
        _connectionMapping.RemoveConnection(userEmail, Context.ConnectionId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        await _gameRoomService.RemoveUserFromRoomAsync(roomId, userEmail);

        _logger.LogInformation("User {User} left room {RoomId}", userEmail, roomId);
    }

    public async Task DrawCard(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);
        _logger.LogInformation("User {User} is drawing a card in room {RoomId}", userEmail, roomId);

        try
        {
            // Draw a card from the game service
            var drawnCard = await _gameService.DrawCardAsync(roomId, userEmail);

            // Notify all players about the drawn card
            await Clients.Client(Context.ConnectionId).SendAsync("CardDrawn", drawnCard);
            await Clients.Group(roomId.ToString()).SendAsync("PlayerDrewCard", userEmail);

            _logger.LogInformation("User {User} drew card {CardType} in room {RoomId}", userEmail, drawnCard, roomId);
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
        _logger.LogInformation("User Identity: {Identity}", Context.User?.Identity?.Name);
        _logger.LogInformation("User IsAuthenticated: {IsAuthenticated}", Context.User?.Identity?.IsAuthenticated);

        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);
        _logger.LogInformation("User email found: {UserEmail}", userEmail);

        try
        {
            // Start the game through the game service
            var gameStatus = await _gameService.StartGameAsync(roomId, userEmail);
            var players = await _gameService.GetPlayersAsync(roomId);

            // Notify all players that the game is starting

            // Send each player their individual card privately
            for (int i = 0; i < players.Count; i++)
            {
                var connectionIds = await GetUserConnectionIds(players[i].UserEmail, roomId);
                foreach (var connectionId in connectionIds)
                {
                    await Clients.Client(connectionId).SendAsync("GameStarted", gameStatus[i]);
                }
            }

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

    private Task<List<string>> GetUserConnectionIds(string userEmail, Guid roomId)
    {
        var connections = _connectionMapping.GetConnections(userEmail).ToList();
        return Task.FromResult(connections);
    }

    public async Task PlayCard(Guid roomId, CardType cardType)
    {
        try
        {
            var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);

            // Verify if it's the user's turn
            var isPlayerTurn = await _gameService.IsPlayerTurnAsync(roomId, userEmail);
            if (!isPlayerTurn)
            {
                await Clients.Caller.SendAsync("PlayCardError", "It's not your turn");
                return;
            }

            // Verify if the card requires more information
            var cardRequirements = await _gameService.GetCardRequirementsAsync(cardType);
            if (cardRequirements.Any(r => r != CardActionRequirements.None))
            {
                // If the card requires more information, request it from the client
                await Clients.Caller.SendAsync("RequestCardInput", cardType, cardRequirements);
                return;
            }

            // If the card does not require more information, apply its effect directly
            var gameStatus = await _gameService.PlayCardAsync(roomId, userEmail, cardType);

            // Send the new status to all the players in the room
            await Clients.Group(roomId.ToString()).SendAsync("CardPlayed", userEmail, cardType);
            await Clients.Group(roomId.ToString()).SendAsync("GameStatusUpdated", gameStatus);

            _logger.LogInformation("Player {UserEmail} played card {CardType} in room {RoomId}", userEmail, cardType, roomId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to play card {CardType} for user {UserEmail} in room {RoomId}", cardType, Context.User?.Identity?.Name, roomId);
            await Clients.Caller.SendAsync("PlayCardError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing card");
            throw new HubException("Failed to play card");
        }
    }

    public async Task InformCardInput(Guid roomId, CardType cardType, object[] additionalInputs)
    {
        try
        {
            var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);

            // Verify if it's the user's turn
            var isPlayerTurn = await _gameService.IsPlayerTurnAsync(roomId, userEmail);
            if (!isPlayerTurn)
            {
                await Clients.Caller.SendAsync("PlayCardError", "It's not your turn");
                return;
            }

            // Play the card with additional inputs
            var gameStatus = await _gameService.PlayCardWithInputAsync(roomId, userEmail, cardType, additionalInputs);

            // Send the new status to all the players in the room
            await Clients.Group(roomId.ToString()).SendAsync("CardPlayed", userEmail, cardType);
            await Clients.Group(roomId.ToString()).SendAsync("GameStatusUpdated", gameStatus);

            _logger.LogInformation("Player {UserEmail} played card {CardType} with additional inputs in room {RoomId}", userEmail, cardType, roomId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to play card {CardType} with inputs for user {UserEmail} in room {RoomId}", cardType, Context.User?.Identity?.Name, roomId);
            await Clients.Caller.SendAsync("PlayCardError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing card with additional inputs");
            throw new HubException("Failed to play card with additional inputs");
        }
    }

    public async Task ReconnectToRoom(Guid roomId)
    {
        try
        {
            var userEmail = Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new HubException("User not authenticated");
            }

            _connectionMapping.AddConnection(userEmail, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            // TODO: Send current game state to the reconnected client
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconnecting to room");
            throw new HubException("Failed to reconnect to room");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove connection from mapping when user disconnects
        _connectionMapping.RemoveConnectionById(Context.ConnectionId);
        
        // SignalR will automatically remove the connection from all groups
        await base.OnDisconnectedAsync(exception);
    }
}
