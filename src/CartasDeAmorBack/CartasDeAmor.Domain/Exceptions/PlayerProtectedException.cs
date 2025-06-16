using System;

namespace CartasDeAmor.Domain.Exceptions;

/// <summary>
/// Exception thrown when a player is protected from effects
/// </summary>
[Serializable]
public class PlayerProtectedException : PlayerException
{
    public DateTime? ProtectionExpiresAt { get; }
    
    public PlayerProtectedException(string message, string playerEmail) 
        : base(message, playerEmail) { }
    
    public PlayerProtectedException(string message, string playerEmail, DateTime protectionExpiresAt) 
        : base(message, playerEmail)
    {
        ProtectionExpiresAt = protectionExpiresAt;
    }
    
    public PlayerProtectedException(string message, Exception innerException) : base(message, innerException) { }
}
