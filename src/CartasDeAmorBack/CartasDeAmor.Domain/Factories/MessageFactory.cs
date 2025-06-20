using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Factories;

public static class MessageFactory
{
    public static SpecialMessage PlayerUpdatePublic(Player player)
    {
        return new SpecialMessage
        {
            Dest = "",
            Message = "PublicPlayerUpdate",
            ExtraData = new
            {
                UserEmail = player.UserEmail,
                Status = player.Status,
                HoldingCardsCount = player.HoldingCards.Count,
                PlayedCards = player.PlayedCards.ToList(),
                Score = player.Score
            }
        };
    }

    public static SpecialMessage PlayerUpdatePrivate(Player player)
    {
        return new SpecialMessage
        {
            Dest = player.UserEmail,
            Message = "PlayerUpdatePrivate",
            ExtraData = new
            {
                UserEmail = player.UserEmail,
                Status = player.Status,
                HoldingCards = player.HoldingCards.ToList(),
                PlayedCards = player.PlayedCards.ToList(),
                Score = player.Score,
            }
        };
    }

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
}