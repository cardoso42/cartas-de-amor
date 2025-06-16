using System;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when authentication fails for game access
/// </summary>
[Serializable]
public class GameAuthenticationException : GameException
{
    public string? AttemptedPassword { get; }
    public string? GameName { get; }
    
    public GameAuthenticationException(string message) : base(message) { }
    
    public GameAuthenticationException(string message, string gameName) 
        : base(message)
    {
        GameName = gameName;
    }
    
    public GameAuthenticationException(string message, string gameName, string attemptedPassword) 
        : base(message)
    {
        GameName = gameName;
        AttemptedPassword = attemptedPassword;
    }
    
    public GameAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}
