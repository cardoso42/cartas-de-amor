using System;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when there's an issue with player actions
/// </summary>
[Serializable]
public class PlayerException : GameException
{
    public string? PlayerEmail { get; }
    
    public PlayerException(string message) : base(message) { }
    
    public PlayerException(string message, string playerEmail) : base(message) 
    {
        PlayerEmail = playerEmail;
    }
    
    public PlayerException(string message, Exception innerException) : base(message, innerException) { }
}
