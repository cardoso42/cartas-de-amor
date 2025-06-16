using System;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when a player doesn't have the required card
/// </summary>
[Serializable]
public class PlayerDoesNotHaveCardException : PlayerException
{
    public CardType RequestedCardType { get; }
    
    public PlayerDoesNotHaveCardException(string message, string playerEmail, CardType requestedCardType) 
        : base(message, playerEmail)
    {
        RequestedCardType = requestedCardType;
    }
    
    public PlayerDoesNotHaveCardException(string message, Exception innerException) 
        : base(message, innerException) { }
}
