using Microsoft.AspNetCore.SignalR;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Exceptions;

namespace CartasDeAmor.Presentation.Hubs;

[Authorize]
public class GameHub(
    ILogger<GameHub> logger, IGameRoomService gameRoomService,
    IGameService gameService, IAccountService accountService,
    IConnectionMappingService connectionMapping) : Hub
{
    private readonly ILogger<GameHub> _logger = logger;
    private readonly IGameRoomService _gameRoomService = gameRoomService;
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
            await Clients.Client(Context.ConnectionId).SendAsync("PrivatePlayerUpdate", playerStatus);
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
                    await Clients.Client(connectionId).SendAsync("RoundStarted", gameStatus[i]);
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
            var requirements = await _gameService.GetCardActionRequirementsAsync(roomId, userEmail, cardType);
            await Clients.Caller.SendAsync("CardRequirements", requirements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting card requirements for card type {CardType}", cardType);
            throw new HubException("Failed to get card requirements");
        }
    }

    private async Task AdvanceGame(Guid roomId)
    {
        var roundWinner = await _gameService.GetRoundWinnerAsync(roomId);
        if (roundWinner != null)
        {
            _logger.LogInformation("Round winner in room {RoomId} is {Winner}", roomId, roundWinner);

            var newRoundData = await _gameService.StartNewRoundAsync(roomId);

            await Clients.Group(roomId.ToString()).SendAsync("RoundWinner", roundWinner);

            var players = (await _gameService.GetPlayersAsync(roomId)).Select(p => p.UserEmail).ToList();
            foreach (var playerEmail in players)
            {
                var connectionIds = await GetUserConnectionIds(playerEmail, roomId);
                foreach (var connectionId in connectionIds)
                {
                    await Clients.Client(connectionId).SendAsync("RoundStarted", newRoundData);
                }
            }
        }

        // TODO: Calling NextPlayerAsync after StartNewRoundAsync makes the next round start with the after the one who won (it should be the one who just won)
        var nextTurnPlayer = await _gameService.NextPlayerAsync(roomId);
        await Clients.Group(roomId.ToString()).SendAsync("NextTurn", nextTurnPlayer);
    }

    public async Task PlayCard(Guid roomId, CardPlayDto cardPlayDto)
    {
        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);
        _logger.LogInformation("User {User} is playing card {CardType} in room {RoomId}", userEmail, cardPlayDto.CardType, roomId);

        try
        {
            var result = await _gameService.PlayCardAsync(roomId, userEmail, cardPlayDto);
            await Clients.Group(roomId.ToString()).SendAsync($"CardResult-{result.Result}", result);

            var invokerPrivateUpdate = await _gameService.GetPlayerStatusAsync(roomId, userEmail);
            await Clients.Client(Context.ConnectionId).SendAsync("PrivatePlayerUpdate", invokerPrivateUpdate);

            if (result.Target != null)
            {
                var targetConnectionIds = await GetUserConnectionIds(result.Target.UserEmail, roomId);
                var targetPrivateUpdate = await _gameService.GetPlayerStatusAsync(roomId, result.Target.UserEmail);

                foreach (var connectionId in targetConnectionIds)
                {
                    await Clients.Client(connectionId).SendAsync("PrivatePlayerUpdate", targetPrivateUpdate);
                }
            }

            _logger.LogInformation("User {User} played card {CardType} in room {RoomId}", userEmail, cardPlayDto.CardType, roomId);

            if (result.Result == CardActionResults.ChooseCard)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ChooseCard", result.CardType);
                return; // Do not advance the game until the user chooses a card
            }

            await AdvanceGame(roomId);
        }
        catch (CardRequirementsNotMetException ex)
        {
            _logger.LogWarning(ex, "Card requirements not met for user {User} in room {RoomId}. Resending them", userEmail, roomId);
            var requirements = await _gameService.GetCardActionRequirementsAsync(roomId, userEmail, cardPlayDto.CardType);
            await Clients.Caller.SendAsync("CardRequirements", requirements);
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
        var userEmail = _accountService.GetEmailFromTokenAsync(Context.User);
        _logger.LogInformation("User {User} is submitting card choice in room {RoomId}", userEmail, roomId);

        try
        {
            var playerUpdate = await _gameService.SubmitCardChoiceAsync(roomId, userEmail, keepCardType, returnCardTypes);
            await Clients.Group(roomId.ToString()).SendAsync("CardChoiceSubmitted", playerUpdate);

            var invokerPrivateUpdate = await _gameService.GetPlayerStatusAsync(roomId, userEmail);
            await Clients.Client(Context.ConnectionId).SendAsync("PrivatePlayerUpdate", invokerPrivateUpdate);

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
