using Microsoft.AspNetCore.Mvc;
using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CartasDeAmor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequestDto createAccountRequest)
    {
        _logger.LogInformation("Attempting to create account for user {Username}", createAccountRequest.Username);

        try
        {
            var accessToken = await _accountService.CreateAccountAsync(
                createAccountRequest.Username,
                createAccountRequest.Email,
                createAccountRequest.Password
            );

            _logger.LogInformation("Successfully created account for user {Username}", createAccountRequest.Username);

            return Ok(new
            {
                AccessToken = accessToken,
                Message = "Account created successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Account creation failed: {Message}", ex.Message);
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create account for user {Username}", createAccountRequest.Username);
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        _logger.LogInformation("Attempting login for user {Username}", loginRequest.Username);

        try
        {
            var accessToken = await _accountService.LoginAsync(
                loginRequest.Username,
                loginRequest.Password
            );

            _logger.LogInformation("Successfully authenticated user {Username}", loginRequest.Username);

            return Ok(new
            {
                AccessToken = accessToken,
                Message = "Login successful."
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed login attempt for user {Username}: {Message}", loginRequest.Username, ex.Message);
            return Unauthorized(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user {Username}", loginRequest.Username);
            throw;
        }
    }

    [Authorize]
    [HttpDelete("{email}")]
    public async Task<IActionResult> DeleteAccount(string email)
    {
        _logger.LogInformation("Claims present: " + string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));
        
        var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userEmail == null)
        {
            _logger.LogWarning("Delete account attempt without valid authentication");
            return Unauthorized(new { Message = "User not properly authenticated" });
        }

        if (!string.Equals(userEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("User {AuthenticatedEmail} attempted to delete account {TargetEmail}", userEmail, email);
            return Forbid();
        }

        _logger.LogInformation("Attempting to delete account with email {Email}", email);

        try
        {
            await _accountService.DeleteAccountAsync(email);
            _logger.LogInformation("Successfully deleted account with email {Email}", email);
            return Ok(new { Message = "Account deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Account deletion failed: {Message}", ex.Message);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting account with email {Email}", email);
            throw;
        }
    }
}