namespace CartasDeAmor.Application.DTOs;

public class LoginResultDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required DateTime Expiration { get; set; } = DateTime.UtcNow.AddHours(1);
    public required string Message { get; set; } = "Login successful";
}
