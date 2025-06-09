namespace CartasDeAmor.Application.DTOs;

public class GameRoomCreationRequestDto
{
    public string RoomName { get; set; } = string.Empty;
    public string? Password { get; set; } = null;
}