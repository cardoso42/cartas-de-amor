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
    public InitialGameStatusDto(Game game, Player player)
    {
        var playersStatuses = new List<PlayerStatusDto>();
        var players = game.Players.ToList();
        players.RemoveAll(p => p.UserEmail == player.UserEmail); // Exclude the current player

        playersStatuses.AddRange(
            players.Select(p => new PlayerStatusDto(p))
        );

        OtherPlayersPublicData = playersStatuses;
        YourCards = player.HoldingCards;
        AllPlayersInOrder = game.Players.Select(p => p.UserEmail).ToList();
        FirstPlayerIndex = game.CurrentPlayerIndex;
        IsProtected = player.IsProtected();
        Score = player.Score;
        CardsRemainingInDeck = game.CardsDeck.Count;
    }
}
