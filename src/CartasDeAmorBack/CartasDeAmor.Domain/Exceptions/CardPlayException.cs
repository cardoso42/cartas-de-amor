using System;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when there's an issue with playing a card
/// </summary>
[Serializable]
public class CardPlayException : GameException
{
    public CardType? CardType { get; }
    
    public CardPlayException(string message) : base(message) { }
    
    public CardPlayException(string message, CardType cardType) : base(message) 
    {
        CardType = cardType;
    }
    
    public CardPlayException(string message, Exception innerException) : base(message, innerException) { }
    
    public CardPlayException(string message, CardType cardType, Exception innerException) : base(message, innerException)
    {
        CardType = cardType;
    }
}
