using CartasDeAmor.Application.DTOs;

namespace CartasDeAmor.Domain.Services;

public interface IGameRoomService
{
    /// <summary>
    /// Retrieves all game rooms.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a collection of game room DTOs</returns>
    Task<IEnumerable<GameRoomDto>> GetAllRoomsAsync();

    /// <summary>
    /// Creates a new room with the specified name and adds the creator as the first player.
    /// </summary>
    /// <param name="name">The name of the room to create.</param>
    /// <param name="creatorEmail">The email of the user creating the room.</param>
    /// <param name="password">An optional password for the room. If provided, the room will be password-protected.</param>
    /// <returns>A task representing the asynchronous operation, containing the ID of the created room.</returns>
    Task<Guid> CreateRoomAsync(string name, string creatorEmail, string? password = null);

    /// <summary>
    /// Deletes a room by its ID.
    /// </summary>
    /// <param name="roomId">The ID of the room to delete.</param>
    /// <param name="userEmail">The email of the user attempting to delete the room, must be the host.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteRoomAsync(Guid roomId, string userEmail);

    /// <summary>
    /// Adds a user to a room.
    /// </summary>
    /// <param name="roomId">The ID of the room to which the user will be added.</param>
    /// <param name="userEmail">The ID of the user to add to the room.</param>
    /// <param name="password">An optional password for the room. If provided, the user must enter the correct password to join.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<JoinRoomResultDto> AddUserToRoomAsync(Guid roomId, string userEmail, string? password);

    /// <summary>
    /// Removes a user from a room.
    /// </summary>
    /// <param name="roomId">The ID of the room from which the user will be removed.</param>
    /// <param name="userEmail">The ID of the user to remove from the room.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveUserFromRoomAsync(Guid roomId, string userEmail);
}