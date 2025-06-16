using System;
using System.Diagnostics.CodeAnalysis;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Base exception class for all game-related exceptions
/// </summary>
[Serializable]
public class GameException : Exception
{
    public GameException() { }
    
    public GameException(string message) : base(message) { }
    
    public GameException(string message, Exception innerException) : base(message, innerException) { }
}
