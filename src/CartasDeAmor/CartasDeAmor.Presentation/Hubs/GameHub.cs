using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Presentation.Hubs;

public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;

    public GameHub(ILogger<GameHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinRoom(Guid roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        _logger.LogInformation("User {User} joined room {RoomId}", Context.User?.FindFirst("sub")?.Value, roomId);
    }

    public async Task LeaveRoom(Guid roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        _logger.LogInformation("User {User} left room {RoomId}", Context.User?.FindFirst("sub")?.Value, roomId);
    }

    public async Task StartGame(Guid roomId)
    {
        throw new NotImplementedException("This method should be implemented to handle game start logic.");
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
        // SignalR will automatically remove the connection from all groups
        await base.OnDisconnectedAsync(exception);
    }
}
