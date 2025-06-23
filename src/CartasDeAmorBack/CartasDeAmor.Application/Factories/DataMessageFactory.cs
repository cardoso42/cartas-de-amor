using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.Factories;

public static class DataMessageFactory
{
    // TODO: Create proper DTOs for these messages instead of using anonymous objects
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

    public static SpecialMessage RoundStart(Game game, Player player)
    {
        return new SpecialMessage
        {
            Dest = player.UserEmail,
            Message = "RoundStarted",
            ExtraData = new InitialGameStatusDto(game, player)
        };
    }

    public static SpecialMessage JoinRoom(Game game, Player player)
    {
        return new SpecialMessage
        {
            Dest = player.UserEmail,
            Message = "JoinedRoom",
            ExtraData = new JoinRoomResultDto(game, player)
        };
    }

    public static SpecialMessage CardRequirements(string player, CardRequirementsDto requirements)
    {
        return new SpecialMessage
        {
            Dest = player,
            Message = "CardRequirements",
            ExtraData = requirements
        };
    }
}