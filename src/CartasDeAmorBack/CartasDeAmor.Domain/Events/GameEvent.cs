using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Events;

/// <summary>
/// Base class for all game events that occur during card plays and game actions
/// </summary>
public abstract class GameEvent
{
    /// <summary>
    /// The type of event (e.g., "PlayCard", "PlayerEliminated", etc.)
    /// </summary>
    public abstract string EventType { get; }
    
    /// <summary>
    /// The destination for this event. Empty string means send to all players in the room.
    /// Specific user email means send only to that user.
    /// </summary>
    public virtual string? Destination => null; // null = all players
    
    /// <summary>
    /// The event data to be sent to clients
    /// </summary>
    public abstract object EventData { get; }
}

/// <summary>
/// Event fired when a player plays a card
/// </summary>
public class PlayCardEvent : GameEvent
{
    public string Player { get; }
    public CardType CardType { get; }

    public PlayCardEvent(string player, CardType cardType)
    {
        Player = player;
        CardType = cardType;
    }

    public override string EventType => "PlayCard";
    public override object EventData => new { Player, CardType };
}

/// <summary>
/// Event fired when a player guesses another player's card
/// </summary>
public class GuessCardEvent : GameEvent
{
    public string Invoker { get; }
    public string Target { get; }
    public CardType CardType { get; }

    public GuessCardEvent(string invoker, string target, CardType cardType)
    {
        Invoker = invoker;
        Target = target;
        CardType = cardType;
    }

    public override string EventType => "GuessCard";
    public override object EventData => new { Invoker, Target, CardType };
}

/// <summary>
/// Event fired when a player peeks at another player's card
/// </summary>
public class PeekCardEvent : GameEvent
{
    public string Invoker { get; }
    public string Target { get; }

    public PeekCardEvent(string invoker, string target)
    {
        Invoker = invoker;
        Target = target;
    }

    public override string EventType => "PeekCard";
    public override object EventData => new { Invoker, Target };
}

/// <summary>
/// Event fired when a player is shown another player's card (private event)
/// </summary>
public class ShowCardEvent : GameEvent
{
    public string Invoker { get; }
    public string Target { get; }
    public CardType CardType { get; }

    public ShowCardEvent(string invoker, string target, CardType cardType)
    {
        Invoker = invoker;
        Target = target;
        CardType = cardType;
    }

    public override string EventType => "ShowCard";
    public override string? Destination => Invoker; // Private event - only to invoker
    public override object EventData => new { Invoker, Target, CardType };
}

/// <summary>
/// Event fired when two players compare their cards
/// </summary>
public class CompareCardsEvent : GameEvent
{
    public string Invoker { get; }
    public string Target { get; }

    public CompareCardsEvent(string invoker, string target)
    {
        Invoker = invoker;
        Target = target;
    }

    public override string EventType => "CompareCards";
    public override object EventData => new { Invoker, Target };
}

/// <summary>
/// Event fired when card comparison results in a tie
/// </summary>
public class ComparisonTieEvent : GameEvent
{
    public string Invoker { get; }
    public string Target { get; }

    public ComparisonTieEvent(string invoker, string target)
    {
        Invoker = invoker;
        Target = target;
    }

    public override string EventType => "ComparisonTie";
    public override object EventData => new { Invoker, Target };
}

/// <summary>
/// Event fired when a player discards a card
/// </summary>
public class DiscardCardEvent : GameEvent
{
    public string Target { get; }
    public CardType CardType { get; }

    public DiscardCardEvent(string target, CardType cardType)
    {
        Target = target;
        CardType = cardType;
    }

    public override string EventType => "DiscardCard";
    public override object EventData => new { Target, CardType };
}

/// <summary>
/// Event fired when a player draws a card
/// </summary>
public class DrawCardEvent : GameEvent
{
    public string Player { get; }

    public DrawCardEvent(string player)
    {
        Player = player;
    }

    public override string EventType => "DrawCard";
    public override object EventData => new { Player };
}

/// <summary>
/// Event fired when a player is eliminated
/// </summary>
public class PlayerEliminatedEvent : GameEvent
{
    public string Player { get; }

    public PlayerEliminatedEvent(string player)
    {
        Player = player;
    }

    public override string EventType => "PlayerEliminated";
    public override object EventData => new { Player };
}

/// <summary>
/// Event fired when two players switch cards
/// </summary>
public class SwitchCardsEvent : GameEvent
{
    public string Invoker { get; }
    public string Target { get; }

    public SwitchCardsEvent(string invoker, string target)
    {
        Invoker = invoker;
        Target = target;
    }

    public override string EventType => "SwitchCards";
    public override object EventData => new { Invoker, Target };
}

/// <summary>
/// Event fired when a player is protected
/// </summary>
public class PlayerProtectedEvent : GameEvent
{
    public string Player { get; }

    public PlayerProtectedEvent(string player)
    {
        Player = player;
    }

    public override string EventType => "PlayerProtected";
    public override object EventData => new { Player };
}

/// <summary>
/// Event fired when a player needs to choose a card
/// </summary>
public class ChooseCardEvent : GameEvent
{
    public string Player { get; }

    public ChooseCardEvent(string player)
    {
        Player = player;
    }

    public override string EventType => "ChooseCard";
    public override object EventData => new { Player };
}

/// <summary>
/// Event fired when cards are returned to the deck
/// </summary>
public class CardReturnedToDeckEvent : GameEvent
{
    public string Player { get; }
    public int CardCount { get; }

    public CardReturnedToDeckEvent(string player, int cardCount)
    {
        Player = player;
        CardCount = cardCount;
    }

    public override string EventType => "CardReturnedToDeck";
    public override object EventData => new { Player, CardCount };
}

/// <summary>
/// Event fired when a player draws a card (alternative to DrawCard)
/// </summary>
public class PlayerDrewCardEvent : GameEvent
{
    public string Player { get; }

    public PlayerDrewCardEvent(string player)
    {
        Player = player;
    }

    public override string EventType => "PlayerDrewCard";
    public override object EventData => Player;
}
