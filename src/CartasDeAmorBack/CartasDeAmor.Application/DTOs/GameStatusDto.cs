using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class GameStatusDto
{
    public ICollection<PublicPlayerUpdateDto> OtherPlayersPublicData { get; set; } = [];
    public PrivatePlayerUpdateDto YourData { get; set; }
    public IList<string> AllPlayersInOrder { get; set; } = [];
    public int FirstPlayerIndex { get; set; } = 0;
    public int CardsRemainingInDeck { get; set; } = 0;

    public GameStatusDto(Game game, ICollection<PublicPlayerUpdateDto> players, Player player)
    {
        OtherPlayersPublicData = players;
        YourData = new PrivatePlayerUpdateDto(player);
        AllPlayersInOrder = game.Players.Select(p => p.UserEmail).ToList();
        FirstPlayerIndex = game.CurrentPlayerIndex;
        CardsRemainingInDeck = game.CardsDeck.Count;
    }
}
