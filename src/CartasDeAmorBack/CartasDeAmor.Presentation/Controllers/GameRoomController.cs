using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Application.DTOs;
using System.Security.Claims;
using CartasDeAmor.Application.Services;

namespace CartasDeAmor.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GameRoomController : ControllerBase
{
    private readonly IGameRoomService _roomService;
    private readonly IAccountService _accountService;
    private readonly ILogger<GameRoomController> _logger;

    public GameRoomController(
        IGameRoomService roomService,
        IAccountService accountService,
        ILogger<GameRoomController> logger)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateRoom([FromBody] GameRoomCreationRequestDto request)
    {
        try
        {
            var userEmail = _accountService.GetEmailFromTokenAsync(User);
            var roomId = await _roomService.CreateRoomAsync(request.RoomName, userEmail, request.Password);
            return Ok(roomId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error creating room");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room");
            return StatusCode(500, "An error occurred while creating the room");
        }
    }

    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteRoom(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromTokenAsync(User);

        try
        {
            await _roomService.DeleteRoomAsync(roomId, userEmail);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Room not found or unauthorized: {RoomId}, {UserEmail}", roomId, userEmail);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room: {RoomId}, {UserEmail}", roomId, userEmail);
            return StatusCode(500, "An error occurred while deleting the room");
        }
    }

    // [HttpPost("{roomId}/users")]
    // public async Task<IActionResult> AddUserToRoom(Guid roomId)
    // {
    //     var userEmail = GetUserEmail();

    //     try
    //     {
    //         await _roomService.AddUserToRoomAsync(roomId, userEmail);
    //         await _notificationService.NotifyUserJoinedRoom(roomId, userEmail);
    //         return Ok();
    //     }
    //     catch (InvalidOperationException ex)
    //     {
    //         _logger.LogWarning(ex, "Error adding user to room: {RoomId}, {UserEmail}", roomId, userEmail);
    //         return BadRequest(ex.Message);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error adding user to room: {RoomId}, {UserEmail}", roomId, userEmail);
    //         return StatusCode(500, "An error occurred while adding user to the room");
    //     }
    // }

    // [HttpDelete("{roomId}/users")]
    // public async Task<IActionResult> RemoveUserFromRoom(Guid roomId)
    // {
    //     var userEmail = GetUserEmail();

    //     try
    //     {
    //         await _roomService.RemoveUserFromRoomAsync(roomId, userEmail);
    //         await _notificationService.NotifyUserLeftRoom(roomId, userEmail);
    //         return Ok();
    //     }
    //     catch (InvalidOperationException ex)
    //     {
    //         _logger.LogWarning(ex, "Error removing user from room: {RoomId}, {UserEmail}", roomId, userEmail);
    //         return BadRequest(ex.Message);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error removing user from room: {RoomId}, {UserEmail}", roomId, userEmail);
    //         return StatusCode(500, "An error occurred while removing user from the room");
    //     }
    // }
}