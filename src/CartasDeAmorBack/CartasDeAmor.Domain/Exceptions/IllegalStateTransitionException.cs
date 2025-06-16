using System;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when changes to the game state are not permitted
/// </summary>
[Serializable]
public class IllegalStateTransitionException : GameException
{
    public string? CurrentState { get; }
    public string? AttemptedState { get; }
    
    public IllegalStateTransitionException(string message) : base(message) { }
    
    public IllegalStateTransitionException(string message, string currentState, string attemptedState) 
        : base(message)
    {
        CurrentState = currentState;
        AttemptedState = attemptedState;
    }
    
    public IllegalStateTransitionException(string message, Exception innerException) : base(message, innerException) { }
}
