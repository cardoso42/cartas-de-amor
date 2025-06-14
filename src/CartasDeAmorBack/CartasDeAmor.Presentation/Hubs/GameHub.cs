using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CartasDeAmor.Presentation.Hubs;

[Authorize]
public class GameHub(
    ILogger<GameHub> logger, IGameRoomService gameRoomService,
    IGameService gameService, IAccountService accountService,
    IConnectionMappingService connectionMapping, ICardService cardService) : Hub
{
    private readonly ILogger<GameHub> _logger = logger;
    private readonly IGameRoomService _gameRoomService = gameRoomService;
    private readonly ICardService _cardService = cardService;
    private readonly IGameService _gameService = gameService;
    private readonly IAccountService _accountService = accountService;
    private readonly IConnectionMappingService _connectionMapping = connectionMapping;

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
            var playerStatus = await _gameService.DrawCardAsync(roomId, userEmail);

            // Notify all players about the drawn card
            await Clients.Client(Context.ConnectionId).SendAsync("CardDrawn", playerStatus);
            await Clients.Group(roomId.ToString()).SendAsync("PlayerDrewCard", userEmail);

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
        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);

        try
        {
            // Start the game through the game service
            var gameStatus = await _gameService.StartGameAsync(roomId, userEmail);
            var players = await _gameService.GetPlayersAsync(roomId);

            // Send each player the initial game status
            for (int i = 0; i < players.Count; i++)
            {
                var connectionIds = await GetUserConnectionIds(players[i].UserEmail, roomId);
                foreach (var connectionId in connectionIds)
                {
                    await Clients.Client(connectionId).SendAsync("GameStarted", gameStatus[i]);
                }
            }

            // Prepare the game for the first player
            await _gameService.NextPlayerAsync(roomId);

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

        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);

        try
        {
            var requirements = await _cardService.GetCardActionRequirementsAsync(roomId, userEmail, cardType);
            await Clients.Caller.SendAsync("CardRequirements", requirements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting card requirements for card type {CardType}", cardType);
            throw new HubException("Failed to get card requirements");
        }
    }

    public async Task PlayCard(Guid roomId, CardType cardType)
    {
        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);
        _logger.LogInformation("User {User} is playing card {CardType} in room {RoomId}", userEmail, cardType, roomId);

        try
        {
            var result = await _gameService.PlayCardAsync(roomId, userEmail, cardType);

            if (result.Success)
            {
                await Clients.Group(roomId.ToString()).SendAsync("CardPlayed", userEmail, cardType);
                await Clients.Client(Context.ConnectionId).SendAsync("CardPlayedSuccess", result);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("RequestCardInput", cardType, result);
            }

            _logger.LogInformation("User {User} played card {CardType} in room {RoomId}", userEmail, cardType, roomId);
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

    private Task<List<string>> GetUserConnectionIds(string userEmail, Guid roomId)
    {
        var connections = _connectionMapping.GetConnections(userEmail).ToList();
        return Task.FromResult(connections);
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
