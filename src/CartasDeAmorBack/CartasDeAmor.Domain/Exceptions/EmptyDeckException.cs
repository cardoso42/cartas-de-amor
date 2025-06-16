using System;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when there are no more cards to draw from the deck
/// </summary>
[Serializable]
public class EmptyDeckException : GameException
{
    public EmptyDeckException() : base("The deck is empty. No more cards can be drawn.") { }
    
    public EmptyDeckException(string message) : base(message) { }
    
    public EmptyDeckException(string message, Exception innerException) : base(message, innerException) { }
}
