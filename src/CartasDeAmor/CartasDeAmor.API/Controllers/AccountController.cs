using Microsoft.AspNetCore.Mvc;
using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Services;
using Microsoft.Extensions.Logging;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create account for user {Username}", createAccountRequest.Username);
            throw;
        }
    }
}