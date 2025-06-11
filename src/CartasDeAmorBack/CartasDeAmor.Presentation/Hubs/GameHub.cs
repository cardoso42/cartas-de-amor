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
    private readonly IConnectionMappingService _connectionMapping;

    public GameHub(ILogger<GameHub> logger, IGameRoomService gameRoomService, IGameService gameService, IConnectionMappingService connectionMapping)
    {
        _logger = logger;
        _gameRoomService = gameRoomService;
        _gameService = gameService;
        _connectionMapping = connectionMapping;
    }

    public async Task JoinRoom(Guid roomId)
    {
        var userEmail = GetUserEmail();

        if (userEmail == null)
        {
            _logger.LogWarning("User email not found in claims when trying to join room {RoomId}", roomId);
            throw new HubException("User not authenticated");
        }

        if (!string.IsNullOrEmpty(userEmail))
        {
            _connectionMapping.AddConnection(userEmail, Context.ConnectionId);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        await _gameRoomService.AddUserToRoomAsync(roomId, userEmail);
        
        _logger.LogInformation("User {User} joined room {RoomId}", userEmail, roomId);
    }

    public async Task LeaveRoom(Guid roomId)
    {
        var userEmail = GetUserEmail();

        if (userEmail == null)
        {
            _logger.LogWarning("User email not found in claims when trying to leave room {RoomId}", roomId);
            throw new HubException("User not authenticated");
        }

        if (!string.IsNullOrEmpty(userEmail))
        {
            _connectionMapping.RemoveConnection(userEmail, Context.ConnectionId);
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        await _gameRoomService.RemoveUserFromRoomAsync(roomId, userEmail);

        _logger.LogInformation("User {User} left room {RoomId}", userEmail, roomId);
    }

    private string? GetUserEmail()
    {
        // Try ClaimTypes.NameIdentifier first (this is what the REST API controllers use)
        var userEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            // Fallback to 'sub' claim
            userEmail = Context.User?.FindFirst("sub")?.Value;
        }
        if (string.IsNullOrEmpty(userEmail))
        {
            // Last resort - try email claim
            userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        }
        return userEmail;
    }

    public async Task StartGame(Guid roomId)
    {
        // Verify if the user is authenticated
        _logger.LogInformation("StartGame called for room {RoomId}", roomId);
        _logger.LogInformation("User Identity: {Identity}", Context.User?.Identity?.Name);
        _logger.LogInformation("User IsAuthenticated: {IsAuthenticated}", Context.User?.Identity?.IsAuthenticated);
        
        // Log all claims for debugging
        if (Context.User?.Claims != null)
        {
            foreach (var claim in Context.User.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
        }
        
        var userEmail = GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("User not authenticated - no valid email claim found");
            throw new HubException("User not authenticated");
        }

        _logger.LogInformation("User email found: {UserEmail}", userEmail);

        try
        {
            // Start the game through the game service
            var gameStatus = await _gameService.StartGameAsync(roomId, userEmail);
            var players = await _gameService.GetPlayersAsync(roomId);

            // Notify all players that the game is starting
            await Clients.Group(roomId.ToString()).SendAsync("GameStarted");

            // Send each player their individual card privately
            for (int i = 0; i < players.Count; i++)
            {
                var connectionIds = await GetUserConnectionIds(players[i].UserEmail, roomId);
                foreach (var connectionId in connectionIds)
                {
                    await Clients.Client(connectionId).SendAsync("PlayerCard", gameStatus[i]);
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

    public Task PlayCard(Guid roomId, CardType cardType)
    {
        try
        {
            var userEmail = Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new HubException("User not authenticated");
            }

            // If card requires more information, request it from the client
            // Else, just apply its effect

            // TODO: Implement game action through GameService
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing card");
            throw new HubException("Failed to play card");
        }

        throw new NotImplementedException("This method should be implemented to handle card playing logic.");
    }

    public Task InformCardInput(Guid roomId)
    {
        throw new NotImplementedException("This method should be implemented to handle card input from the client.");
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
