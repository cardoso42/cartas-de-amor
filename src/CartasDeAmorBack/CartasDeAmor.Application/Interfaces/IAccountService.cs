using System.Security.Claims;
using CartasDeAmor.Application.DTOs;

namespace CartasDeAmor.Domain.Services;

public interface IAccountService
{
    /// <summary>
    /// Creates a new account with the provided details.
    /// </summary>
    /// <param name="username">The username for the new account.</param>
    /// <param name="email">The email address associated with the new account.</param>
    /// <param name="password">The password for the new account.</param>
    /// <returns>A task representing the asynchronous operation, containing a boolean indicating success or failure.</returns>
    Task<string> CreateAccountAsync(string username, string email, string password);

    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    /// </summary>
    /// <param name="email">The email of the account.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>A JWT token if authentication is successful, or throws an exception if it fails.</returns>
    Task<LoginResultDto> LoginAsync(string email, string password);

    /// <summary>
    /// Updates a user account's username.
    /// </summary>
    /// <param name="email">The email of the user to update.</param>
    /// <param name="username">The new username for the account.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAccountAsync(string email, string username);

    /// <summary>
    /// Deletes a user account.
    /// </summary>
    /// <param name="email">The email of the user to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAccountAsync(string email);

    /// <summary>
    /// Retrieves the email address from the user's claims.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal representing the user.</param>
    /// <returns>The email address.</returns>
    /// <remarks>
    /// This method extracts the email from the user's claims, which is typically set during authentication.
    /// If the user is not authenticated or the email claim is not present, it throws an exception.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the email claim is not found in the user's claims.</exception>
    public string GetEmailFromToken(ClaimsPrincipal? user);
}
