using System;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when a player is not eligible for a certain action
/// </summary>
[Serializable]
public class PlayerNotEligibleException : PlayerException
{
    public PlayerStatus? PlayerStatus { get; }
    
    public PlayerNotEligibleException(string message) : base(message) { }
    
    public PlayerNotEligibleException(string message, string playerEmail) : base(message, playerEmail) { }
    
    public PlayerNotEligibleException(string message, string playerEmail, PlayerStatus playerStatus) 
        : base(message, playerEmail)
    {
        PlayerStatus = playerStatus;
    }
    
    public PlayerNotEligibleException(string message, Exception innerException) : base(message, innerException) { }
}
