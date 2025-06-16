using System;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to play an invalid card
/// </summary>
[Serializable]
public class InvalidCardPlayException : CardPlayException
{
    public CardType PlayedCardType { get; }
    public CardType? ConflictingCardType { get; }
    
    public InvalidCardPlayException(string message, CardType playedCardType) 
        : base(message, playedCardType)
    {
        PlayedCardType = playedCardType;
    }
    
    public InvalidCardPlayException(string message, CardType playedCardType, CardType conflictingCardType) 
        : base(message, playedCardType)
    {
        PlayedCardType = playedCardType;
        ConflictingCardType = conflictingCardType;
    }
    
    public InvalidCardPlayException(string message, Exception innerException) : base(message, innerException) { }
}
