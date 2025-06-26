using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CartasDeAmor.Domain.Services;
using CartasDeAmor.Application.DTOs;

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
            var userEmail = _accountService.GetEmailFromToken(User);
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameRoomDto>>> GetAvailableRooms()
    {
        try
        {
            var rooms = await _roomService.GetAvailableRooms();
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rooms");
            return StatusCode(500, "An error occurred while retrieving the rooms");
        }
    }

    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<GameRoomDto>>> GetRoomsFromUser()
    {
        var userEmail = _accountService.GetEmailFromToken(User);

        try
        {
            var rooms = await _roomService.GetActiveRoomsByUserAsync(userEmail);
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rooms for user: {UserEmail}", userEmail);
            return StatusCode(500, "An error occurred while retrieving the user's rooms");
        }
    }

    [HttpDelete("{roomId}")]
    public async Task<IActionResult> DeleteRoom(Guid roomId)
    {
        var userEmail = _accountService.GetEmailFromToken(User);

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
}