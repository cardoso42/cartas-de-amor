using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Factories;

public static class EventMessageFactory
{
    public static SpecialMessage PlayCard(string player, CardType cardType)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "PlayCard",
            ExtraData = new
            {
                Player = player,
                CardType = cardType
            }
        };
    }

    public static SpecialMessage GuessCard(string invoker, string target, CardType cardType)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "GuessCard",
            ExtraData = new
            {
                Invoker = invoker,
                CardType = cardType,
                Target = target
            }
        };
    }

    public static SpecialMessage PeekCard(string invoker, string target)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "PeekCard",
            ExtraData = new
            {
                Invoker = invoker,
                Target = target
            }
        };
    }

    public static SpecialMessage ShowCard(string invoker, string target, CardType cardType)
    {
        return new SpecialMessage
        {
            Dest = invoker,
            Message = "ShowCard",
            ExtraData = new
            {
                Invoker = invoker,
                Target = target,
                CardType = cardType
            }
        };
    }

    public static SpecialMessage CompareCards(string invoker, string target)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "CompareCards",
            ExtraData = new
            {
                Invoker = invoker,
                Target = target
            }
        };
    }

    public static SpecialMessage ComparisonTie(string invoker, string target)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "ComparisonTie",
            ExtraData = new
            {
                Invoker = invoker,
                Target = target
            }
        };
    }

    public static SpecialMessage DiscardCard(string target, CardType cardType)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "DiscardCard",
            ExtraData = new
            {
                Target = target,
                CardType = cardType
            }
        };
    }

    public static SpecialMessage DrawCard(string player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "DrawCard",
            ExtraData = new
            {
                Player = player,
            }
        };
    }

    public static SpecialMessage PlayerEliminated(string player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "PlayerEliminated",
            ExtraData = new
            {
                Player = player,
            }
        };
    }

    public static SpecialMessage SwitchCards(string invoker, string target)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "SwitchCards",
            ExtraData = new
            {
                Invoker = invoker,
                Target = target
            }
        };
    }

    public static SpecialMessage PlayerProtected(string player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "PlayerProtected",
            ExtraData = new
            {
                Player = player,
            }
        };
    }

    public static SpecialMessage ChooseCard(string player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "ChooseCard",
            ExtraData = new
            {
                Player = player,
            }
        };
    }

    public static SpecialMessage CardReturnedToDeck(string player, int cardCount)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "CardReturnedToDeck",
            ExtraData = new
            {
                Player = player,
                CardCount = cardCount
            }
        };
    }

    public static SpecialMessage PlayerDrewCard(string player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "PlayerDrewCard",
            ExtraData = player
        };
    }

    public static SpecialMessage UserJoined(string player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "UserJoined",
            ExtraData = player
        };
    }

    public static SpecialMessage UserLeft(string player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "UserLeft",
            ExtraData = player
        };
    }

    public static SpecialMessage RoundWinners(List<string> winners)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "RoundWinners",
            ExtraData = winners
        };
    }

    public static SpecialMessage BonusPoints(List<string> bonusPointsReceivers)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "BonusPoints",
            ExtraData = bonusPointsReceivers
        };
    }

    public static SpecialMessage GameOver(List<string> winners)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "GameOver",
            ExtraData = winners
        };
    }

    public static SpecialMessage NextTurn(string player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "NextTurn",
            ExtraData = player
        };
    }

    public static SpecialMessage ReturnCards(string player, int cardsCount)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "CardReturnedToDeck",
            ExtraData = new
            {
                Player = player,
                CardCount = cardsCount
            }
        };
    }
}