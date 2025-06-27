using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class InitialGameStatusDto
{
    public ICollection<PlayerStatusDto> OtherPlayersPublicData { get; set; } = [];
    public ICollection<CardType> YourCards { get; set; } = [];
    public bool IsProtected { get; set; } = false;
    public IList<string> AllPlayersInOrder { get; set; } = [];
    public int FirstPlayerIndex { get; set; } = 0;
    public int Score { get; set; } = 0;
    public int CardsRemainingInDeck { get; set; } = 0;

    public InitialGameStatusDto() { }
    public InitialGameStatusDto(Game game, ICollection<PlayerStatusDto> players, Player player)
    {
        OtherPlayersPublicData = players;
        YourCards = player.HoldingCards;
        AllPlayersInOrder = game.Players.Select(p => p.UserEmail).ToList();
        FirstPlayerIndex = game.CurrentPlayerIndex;
        IsProtected = player.IsProtected();
        Score = player.Score;
        CardsRemainingInDeck = game.CardsDeck.Count;
    }
}
