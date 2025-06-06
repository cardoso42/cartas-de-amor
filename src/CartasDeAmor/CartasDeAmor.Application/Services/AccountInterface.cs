using CartasDeAmor.Application.Interfaces;
using CartasDeAmor.Infrastructure.Repositories;
using BCrypt.Net;

namespace CartasDeAmor.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;

    public AccountService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public bool CreateAccount(string username, string password)
    {
        // 1. Check if user exists
        if (_userRepository.UserExists(username))
        {
            return false; // User already exists
        }

        // 2. Hash password securely using BCrypt
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        // 3. Insert user into database
        _userRepository.AddUser(username, hashedPassword);

        // 4. Return success
        return true;
    }
}
