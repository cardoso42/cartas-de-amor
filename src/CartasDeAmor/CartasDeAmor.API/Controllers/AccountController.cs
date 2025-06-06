using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CartasDeAmor.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountController(IAccountService accountService) : ControllerBase
{
    // POST: api/accounts/register
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto registerDto)
    {
        if (string.IsNullOrEmpty(registerDto.Username) || string.IsNullOrEmpty(registerDto.Password))
        {
            return BadRequest(new { message = "Invalid registration data." });
        }
        
        var success = accountService.CreateAccount(registerDto.Username, registerDto.Password);
        
        return Created("", new { message = "User registered successfully." });
    }

    // POST: api/accounts/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto loginDto)
    {
        return Ok(new { token = "fake-jwt-token" });
    }

    // DELETE: api/accounts
    [HttpDelete]
    public IActionResult DeleteAccount([FromBody] DeleteAccountDto deleteDto)
    {
        return NoContent();
    }
}
