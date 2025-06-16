using System;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when an action is attempted on an invalid turn
/// </summary>
[Serializable]
public class InvalidTurnException : GameException
{
    public string? ExpectedPlayerEmail { get; }
    public string? ActualPlayerEmail { get; }
    
    public InvalidTurnException(string message) : base(message) { }
    
    public InvalidTurnException(string message, string expectedPlayerEmail, string actualPlayerEmail) 
        : base(message)
    {
        ExpectedPlayerEmail = expectedPlayerEmail;
        ActualPlayerEmail = actualPlayerEmail;
    }
    
    public InvalidTurnException(string message, Exception innerException) : base(message, innerException) { }
}
