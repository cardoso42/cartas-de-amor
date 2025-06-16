using System;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when a game has reached its maximum player capacity
/// </summary>
[Serializable]
public class GameFullException : GameException
{
    public int MaxPlayers { get; }
    
    public GameFullException(int maxPlayers) 
        : base($"The game has reached its maximum capacity of {maxPlayers} players.")
    {
        MaxPlayers = maxPlayers;
    }
    
    public GameFullException(string message, int maxPlayers) : base(message) 
    {
        MaxPlayers = maxPlayers;
    }
    
    public GameFullException(string message, Exception innerException) : base(message, innerException) { }
}
