using MediatR;
using Microsoft.AspNetCore.SignalR;
using CartasDeAmor.Application.Commands;
using CartasDeAmor.Presentation.Hubs;

namespace CartasDeAmor.Presentation.Handlers;

/// <summary>
/// Handler for username changed notifications
/// </summary>
public class SendUsernameChangedHandler : IRequestHandler<SendUsernameChangedCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendUsernameChangedHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendUsernameChangedCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All
            .SendAsync("UsernameChanged", new { UserEmail = request.UserEmail, NewUsername = request.NewUsername }, cancellationToken);
    }
}

/// <summary>
/// Handler for user joined room notifications
/// </summary>
public class SendUserJoinedRoomHandler : IRequestHandler<SendUserJoinedRoomCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendUserJoinedRoomHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendUserJoinedRoomCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(request.RoomId)
            .SendAsync("UserJoined", request.UserEmail, cancellationToken);
    }
}

/// <summary>
/// Handler for user left room notifications
/// </summary>
public class SendUserLeftRoomHandler : IRequestHandler<SendUserLeftRoomCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendUserLeftRoomHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendUserLeftRoomCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(request.RoomId)
            .SendAsync("UserLeft", request.UserEmail, cancellationToken);
    }
}

/// <summary>
/// Handler for generic SignalR messages
/// </summary>
public class SendGenericSignalRMessageHandler : IRequestHandler<SendGenericSignalRMessageCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendGenericSignalRMessageHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendGenericSignalRMessageCommand request, CancellationToken cancellationToken)
    {
        switch (request.TargetType)
        {
            case "Room":
                await _hubContext.Clients.Group(request.Target)
                    .SendAsync(request.MessageType, request.Args, cancellationToken);
                break;
            case "User":
                await _hubContext.Clients.User(request.Target)
                    .SendAsync(request.MessageType, request.Args, cancellationToken);
                break;
        }
    }
}

// Game Event Handlers

/// <summary>
/// Handler for player update (public) messages
/// </summary>
public class SendPlayerUpdatePublicHandler : IRequestHandler<SendPlayerUpdatePublicCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendPlayerUpdatePublicHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendPlayerUpdatePublicCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(request.RoomId.ToString())
            .SendAsync("PublicPlayerUpdate", new
            {
                UserEmail = request.UserEmail,
                Status = request.Status,
                HoldingCardsCount = request.HoldingCardsCount,
                PlayedCards = request.PlayedCards,
                Score = request.Score
            }, cancellationToken);
    }
}

/// <summary>
/// Handler for player update (private) messages
/// </summary>
public class SendPlayerUpdatePrivateHandler : IRequestHandler<SendPlayerUpdatePrivateCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendPlayerUpdatePrivateHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendPlayerUpdatePrivateCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.User(request.UserEmail)
            .SendAsync("PlayerUpdatePrivate", new
            {
                UserEmail = request.UserEmail,
                Status = request.Status,
                HoldingCards = request.HoldingCards,
                PlayedCards = request.PlayedCards,
                Score = request.Score
            }, cancellationToken);
    }
}

/// <summary>
/// Handler for round start messages
/// </summary>
public class SendRoundStartHandler : IRequestHandler<SendRoundStartCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendRoundStartHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendRoundStartCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.User(request.UserEmail)
            .SendAsync("RoundStarted", request.GameData, cancellationToken);
    }
}

/// <summary>
/// Handler for join room result messages
/// </summary>
public class SendJoinRoomHandler : IRequestHandler<SendJoinRoomCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendJoinRoomHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendJoinRoomCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.User(request.UserEmail)
            .SendAsync("JoinedRoom", request.JoinRoomData, cancellationToken);
    }
}

/// <summary>
/// Handler for card requirements messages
/// </summary>
public class SendCardRequirementsHandler : IRequestHandler<SendCardRequirementsCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendCardRequirementsHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendCardRequirementsCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.User(request.UserEmail)
            .SendAsync("CardRequirements", request.RequirementsData, cancellationToken);
    }
}

/// <summary>
/// Handler for next turn messages
/// </summary>
public class SendNextTurnHandler : IRequestHandler<SendNextTurnCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendNextTurnHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendNextTurnCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(request.RoomId.ToString())
            .SendAsync("NextTurn", request.Player, cancellationToken);
    }
}

/// <summary>
/// Handler for general game events
/// </summary>
public class SendGameEventHandler : IRequestHandler<SendGameEventCommand>
{
    private readonly IHubContext<GameHub> _hubContext;

    public SendGameEventHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(SendGameEventCommand request, CancellationToken cancellationToken)
    {
        if (request.DestinationUser == null)
        {
            // Send to all players in room
            await _hubContext.Clients.Group(request.RoomId.ToString())
                .SendAsync(request.EventType, request.EventData, cancellationToken);
        }
        else
        {
            // Send to specific user
            await _hubContext.Clients.User(request.DestinationUser)
                .SendAsync(request.EventType, request.EventData, cancellationToken);
        }
    }
}
