using System;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when a mandatory card play rule is violated
/// </summary>
[Serializable]
public class MandatoryCardPlayViolationException : CardPlayException
{
    public CardType PlayedCardType { get; }
    public CardType RequiredCardType { get; }
    
    public MandatoryCardPlayViolationException(
        string message, 
        CardType playedCardType, 
        CardType requiredCardType) 
        : base(message, playedCardType)
    {
        PlayedCardType = playedCardType;
        RequiredCardType = requiredCardType;
    }
    
    public MandatoryCardPlayViolationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
