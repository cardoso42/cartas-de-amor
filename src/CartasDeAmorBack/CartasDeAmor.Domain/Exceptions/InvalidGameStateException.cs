using System;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when an action is not allowed in the current game state
/// </summary>
[Serializable]
public class InvalidGameStateException : GameException
{
    public GameStateEnum CurrentState { get; }
    public GameStateEnum? RequiredState { get; }
    
    public InvalidGameStateException(string message, GameStateEnum currentState) 
        : base(message)
    {
        CurrentState = currentState;
    }
    
    public InvalidGameStateException(string message, GameStateEnum currentState, GameStateEnum requiredState) 
        : base(message)
    {
        CurrentState = currentState;
        RequiredState = requiredState;
    }
    
    public InvalidGameStateException(string message, Exception innerException) : base(message, innerException) { }
}
