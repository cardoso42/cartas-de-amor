using MediatR;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.Commands;

/// <summary>
/// Specific command for username changed notifications
/// </summary>
public class SendUsernameChangedCommand : IRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public string NewUsername { get; set; } = string.Empty;
}

/// <summary>
/// Specific command for user joined room notifications
/// </summary>
public class SendUserJoinedRoomCommand : IRequest
{
    public string RoomId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}

/// <summary>
/// Specific command for user left room notifications
/// </summary>
public class SendUserLeftRoomCommand : IRequest
{
    public string RoomId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}

/// <summary>
/// Generic command for other SignalR messages with flexible arguments
/// </summary>
public class SendGenericSignalRMessageCommand : IRequest
{
    public string Target { get; set; } = string.Empty; // Room ID or User Email
    public string TargetType { get; set; } = string.Empty; // "Room", "User", or "UserRooms"
    public string MessageType { get; set; } = string.Empty;
    public object[] Args { get; set; } = Array.Empty<object>();
}

// Game Event Commands

/// <summary>
/// Command for player update (public) messages
/// </summary>
public class SendPlayerUpdatePublicCommand : IRequest
{
    public Guid RoomId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public PlayerStatus Status { get; set; }
    public int HoldingCardsCount { get; set; }
    public List<CardType> PlayedCards { get; set; } = [];
    public int Score { get; set; }
}

/// <summary>
/// Command for player update (private) messages
/// </summary>
public class SendPlayerUpdatePrivateCommand : IRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public PlayerStatus Status { get; set; }
    public List<CardType> HoldingCards { get; set; } = [];
    public List<CardType> PlayedCards { get; set; } = [];
    public int Score { get; set; }
}

/// <summary>
/// Command for round start messages
/// </summary>
public class SendRoundStartCommand : IRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public object GameData { get; set; } = new();
}

/// <summary>
/// Command for join room result messages
/// </summary>
public class SendJoinRoomCommand : IRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public object JoinRoomData { get; set; } = new();
}

/// <summary>
/// Command for card requirements messages
/// </summary>
public class SendCardRequirementsCommand : IRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public object RequirementsData { get; set; } = new();
}

/// <summary>
/// Command for next turn messages
/// </summary>
public class SendNextTurnCommand : IRequest
{
    public Guid RoomId { get; set; }
    public string Player { get; set; } = string.Empty;
}

/// <summary>
/// Command for game events (card plays, eliminations, etc.)
/// </summary>
public class SendGameEventCommand : IRequest
{
    public Guid RoomId { get; set; }
    public string? DestinationUser { get; set; } // null for all players in room
    public string EventType { get; set; } = string.Empty;
    public object EventData { get; set; } = new();
}
