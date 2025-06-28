using CartasDeAmor.Application.Commands;
using CartasDeAmor.Domain.Entities;
using MediatR;

namespace CartasDeAmor.Application.Extensions;

/// <summary>
/// Extension methods for IMediator to easily send strongly-typed SignalR messages from application layer
/// </summary>
public static class SignalRMediatorExtensions
{
    /// <summary>
    /// Send a username changed notification to all user rooms
    /// </summary>
    public static async Task SendUsernameChangedAsync(this IMediator mediator, string userEmail, string newUsername, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SendUsernameChangedCommand
        {
            UserEmail = userEmail,
            NewUsername = newUsername
        }, cancellationToken);
    }
    
    /// <summary>
    /// Send a generic SignalR message with flexible arguments
    /// </summary>
    public static async Task SendGenericSignalRMessageAsync(this IMediator mediator, string target, string targetType, string messageType, params object[] args)
    {
        await mediator.Send(new SendGenericSignalRMessageCommand
        {
            Target = target,
            TargetType = targetType,
            MessageType = messageType,
            Args = args
        });
    }

    /// <summary>
    /// Send next turn notification
    /// </summary>
    public static async Task SendNextTurnAsync(this IMediator mediator, Guid roomId, string player, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SendNextTurnCommand
        {
            RoomId = roomId,
            Player = player
        }, cancellationToken);
    }

    /// <summary>
    /// Send player update (public) notification
    /// </summary>
    public static async Task SendPlayerUpdatePublicAsync(this IMediator mediator, Guid roomId, Player player, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SendPlayerUpdatePublicCommand
        {
            RoomId = roomId,
            UserEmail = player.UserEmail,
            Status = player.Status,
            HoldingCardsCount = player.HoldingCards.Count(),
            PlayedCards = player.PlayedCards.ToList(),
            Score = player.Score
        }, cancellationToken);
    }

    /// <summary>
    /// Send player update (private) notification
    /// </summary>
    public static async Task SendPlayerUpdatePrivateAsync(this IMediator mediator, Player player, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SendPlayerUpdatePrivateCommand
        {
            UserEmail = player.UserEmail,
            Status = player.Status,
            HoldingCards = player.HoldingCards.ToList(),
            PlayedCards = player.PlayedCards.ToList(),
            Score = player.Score
        }, cancellationToken);
    }

    /// <summary>
    /// Send join room result notification
    /// </summary>
    public static async Task SendJoinRoomAsync(this IMediator mediator, string userEmail, object joinRoomData, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SendJoinRoomCommand
        {
            UserEmail = userEmail,
            JoinRoomData = joinRoomData
        }, cancellationToken);
    }

    /// <summary>
    /// Send card requirements notification
    /// </summary>
    public static async Task SendCardRequirementsAsync(this IMediator mediator, string userEmail, object requirementsData, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SendCardRequirementsCommand
        {
            UserEmail = userEmail,
            RequirementsData = requirementsData
        }, cancellationToken);
    }

    /// <summary>
    /// Send round start notification
    /// </summary>
    public static async Task SendRoundStartAsync(this IMediator mediator, string userEmail, object gameData, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SendRoundStartCommand
        {
            UserEmail = userEmail,
            GameData = gameData
        }, cancellationToken);
    }

    /// <summary>
    /// Send game event notification
    /// </summary>
    public static async Task SendGameEventAsync(this IMediator mediator, Guid roomId, string? destinationUser, string eventType, object eventData, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SendGameEventCommand
        {
            RoomId = roomId,
            DestinationUser = destinationUser,
            EventType = eventType,
            EventData = eventData
        }, cancellationToken);
    }
}
