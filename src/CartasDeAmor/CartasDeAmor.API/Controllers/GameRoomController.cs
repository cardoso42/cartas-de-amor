using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Application.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CartasDeAmor.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GameRoomController : ControllerBase
{
    private readonly IGameRoomService _roomService;
    private readonly ILogger<GameRoomController> _logger;

    public GameRoomController(IGameRoomService roomService, ILogger<GameRoomController> logger)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string GetUserEmail()
    {
        var userEmail =  User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new InvalidOperationException("User email not found in claims");
        return userEmail;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateRoom([FromBody] GameRoomCreationRequestDto request)
    {
        try
        {
            var userEmail = GetUserEmail();
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
        var userEmail = GetUserEmail();

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

    [HttpPost("{roomId}/users")]
    public async Task<IActionResult> AddUserToRoom(Guid roomId)
    {
        var userEmail = GetUserEmail();

        try
        {
            await _roomService.AddUserToRoomAsync(roomId, userEmail);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error adding user to room: {RoomId}, {UserEmail}", roomId, userEmail);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user to room: {RoomId}, {UserEmail}", roomId, userEmail);
            return StatusCode(500, "An error occurred while adding user to the room");
        }
    }

    [HttpDelete("{roomId}/users")]
    public async Task<IActionResult> RemoveUserFromRoom(Guid roomId)
    {
        var userEmail = GetUserEmail(); // Verify current user is authenticated

        try
        {
            await _roomService.RemoveUserFromRoomAsync(roomId, userEmail);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error removing user from room: {RoomId}, {UserEmail}", roomId, userEmail);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user from room: {RoomId}, {UserEmail}", roomId, userEmail);
            return StatusCode(500, "An error occurred while removing user from the room");
        }
    }
}