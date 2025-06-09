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
}