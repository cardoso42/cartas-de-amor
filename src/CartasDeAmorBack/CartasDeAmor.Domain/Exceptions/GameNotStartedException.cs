using System;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when a game has reached its maximum player capacity
/// </summary>
[Serializable]
public class GameNotStartedException : GameException
{    
    public GameNotStartedException() 
        : base($"The game has not started yet. Please wait for the game to begin.")
    {
    }
    
    public GameNotStartedException(string message) : base(message) 
    {
    }
    
    public GameNotStartedException(string message, Exception innerException) : base(message, innerException) { }
}
