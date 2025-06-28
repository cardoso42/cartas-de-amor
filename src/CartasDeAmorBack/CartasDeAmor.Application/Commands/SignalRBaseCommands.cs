using MediatR;

namespace CartasDeAmor.Application.Commands;

/// <summary>
/// Base interface for all SignalR message commands
/// </summary>
public interface ISignalRCommand : IRequest
{
    string MessageType { get; }
}

/// <summary>
/// Command to send a message to all users in a specific room
/// </summary>
public class SendRoomMessageCommand : ISignalRCommand
{
    public string RoomId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public object[] Args { get; set; } = Array.Empty<object>();
}

/// <summary>
/// Command to send a message to a specific user
/// </summary>
public class SendUserMessageCommand : ISignalRCommand
{
    public string UserEmail { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public object[] Args { get; set; } = Array.Empty<object>();
}

/// <summary>
/// Command to send a message to all rooms that a user is part of
/// </summary>
public class SendUserRoomsMessageCommand : ISignalRCommand
{
    public string UserEmail { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public object[] Args { get; set; } = Array.Empty<object>();
}
