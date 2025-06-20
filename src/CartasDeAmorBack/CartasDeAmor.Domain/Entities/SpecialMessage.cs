using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

/// <summary>
/// Represents a special message that can be sent to a player during the game.
/// while the `Message` property is required to specify the content of the 
/// message. The `ExtraData` property can be used to include additional 
/// information relevant to the message.
/// </summary>
/// <remarks>
/// If Dest is empty, the message is sent to all players.
/// </remarks>
public class SpecialMessage
{
    /// <summary>
    /// Destination player for the message. 
    /// If empty, the message is sent to all players.
    /// </summary>
    public required string Dest { get; set; }

    /// <summary>
    /// Content of the message to be sent.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Additional data relevant to the message.
    /// Can be null if no extra information is needed.
    /// </summary>
    public object? ExtraData { get; set; } = null;

    public SpecialMessage() { }
}

public class CardGuessMessage : SpecialMessage
{
    public CardGuessMessage(string guessingEmail, CardType cardType, string guessedEmail)
    {
        Dest = "";
        Message = "GuessCard";
        ExtraData = new
        {
            GuessingEmail = guessingEmail,
            CardType = cardType,
            GuessedEmail = guessedEmail
        };
    }
}
