using Microsoft.AspNetCore.SignalR;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using CartasDeAmor.Domain.Exceptions;
using CartasDeAmor.Domain.Entities;

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

    private async Task SendSpecialMessage(Guid roomId, SpecialMessage message)
    {
        if (string.IsNullOrEmpty(message.Dest))
        {
            await Clients.Group(roomId.ToString()).SendAsync(message.Message, message.ExtraData);
        }
        else
        {
            await Clients.User(message.Dest).SendAsync(message.Message, message.ExtraData);
        }
    }
    
    private async Task SendSpecialMessages(Guid roomId, List<SpecialMessage> messages)
    {
        foreach (var message in messages)
        {
            await SendSpecialMessage(roomId, message);
        }
    }

    public async Task JoinRoom(Guid roomId, string? password)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _connectionMapping.AddConnection(userEmail, Context.ConnectionId);

        var messages = await _gameRoomService.AddUserToRoomAsync(roomId, userEmail, password);

        await SendSpecialMessages(roomId, messages);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        
        _logger.LogInformation("User {User} joined room {RoomId}", userEmail, roomId);
    }

    public async Task LeaveRoom(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _connectionMapping.RemoveConnection(userEmail, Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        var messages = await _gameRoomService.RemoveUserFromRoomAsync(roomId, userEmail);

        await SendSpecialMessages(roomId, messages);

        _logger.LogInformation("User {User} left room {RoomId}", userEmail, roomId);
    }

    public async Task DrawCard(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _logger.LogInformation("User {User} is drawing a card in room {RoomId}", userEmail, roomId);

        try
        {
            var messages = await _gameService.DrawCardAsync(roomId, userEmail);
            await SendSpecialMessages(roomId, messages);
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
            var messages = await _gameService.StartGameAsync(roomId, userEmail);
            await SendSpecialMessages(roomId, messages);

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

        var userEmail = _accountService.GetEmailFromToken(Context.User);

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
        if (await _gameService.IsRoundOverAsync(roomId))
        {
            var roundWinners = await _gameService.FinishRoundAsync(roomId);
            _logger.LogInformation("Round winner in room {RoomId} is {Winner}", roomId, roundWinners);
            await Clients.Group(roomId.ToString()).SendAsync("RoundWinners", roundWinners);

            var bonusPointsReceivers = await _gameService.AddBonusPointsAsync(roomId);
            _logger.LogInformation("Bonus points awarded to players in room {RoomId}: {Receivers}", roomId, bonusPointsReceivers);
            await Clients.Group(roomId.ToString()).SendAsync("BonusPoints", bonusPointsReceivers);

            if (await _gameService.IsGameOverAsync(roomId))
            {
                var gameWinners = await _gameService.FinishGameAsync(roomId);

                _logger.LogInformation("Game winner in room {RoomId} is {Winner}", roomId, gameWinners);
                await Clients.Group(roomId.ToString()).SendAsync("GameOver", gameWinners);

                return; // No need to start a new round if the game is over
            }

            var messages = await _gameService.StartNewRoundAsync(roomId);
            await SendSpecialMessages(roomId, messages);
        }

        // TODO: Calling NextPlayerAsync after StartNewRoundAsync makes the next round start with the after the one who won (it should be the one who just won)
        var nextTurnPlayer = await _gameService.NextPlayerAsync(roomId);
        await Clients.Group(roomId.ToString()).SendAsync("NextTurn", nextTurnPlayer);
    }

    public async Task PlayCard(Guid roomId, CardPlayDto cardPlayDto)
    {
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _logger.LogInformation("User {User} is playing card {CardType} in room {RoomId}", userEmail, cardPlayDto.CardType, roomId);

        try
        {
            var result = await _gameService.PlayCardAsync(roomId, userEmail, cardPlayDto);

            await SendSpecialMessages(roomId, result.SpecialMessages);
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
        var userEmail = _accountService.GetEmailFromToken(Context.User);
        _logger.LogInformation("User {User} is submitting card choice in room {RoomId}", userEmail, roomId);

        try
        {
            var playerUpdate = await _gameService.SubmitCardChoiceAsync(roomId, userEmail, keepCardType, returnCardTypes);
            await Clients.Group(roomId.ToString()).SendAsync("CardChoiceSubmitted", playerUpdate);

            // Send CardReturnedToDeck event when cards are returned to deck
            if (returnCardTypes.Count > 0)
            {
                await Clients.Group(roomId.ToString()).SendAsync("CardReturnedToDeck", new { Player = userEmail, CardCount = returnCardTypes.Count });
            }

            var invokerPrivateUpdate = await _gameService.GetPlayerStatusAsync(roomId, userEmail);
            await Clients.Client(Context.ConnectionId).SendAsync("PlayerUpdatePrivate", invokerPrivateUpdate);
            // TODO: await Clients.Group(roomId.ToString()).SendAsync("PlayerUpdatePublic", _gameService.GetPublicPlayerStatus(roomId, userEmail));

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

    private List<string> GetUserConnectionIds(string userEmail, Guid roomId)
    {
        var connections = _connectionMapping.GetConnections(userEmail).ToList();
        return connections;
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
