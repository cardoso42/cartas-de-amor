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
    /// <param name="username">The username of the account.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>A JWT token if authentication is successful, or throws an exception if it fails.</returns>
    Task<string> LoginAsync(string username, string password);

    /// <summary>
    /// Deletes a user account.
    /// </summary>
    /// <param name="email">The email of the user to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAccountAsync(string email);
}