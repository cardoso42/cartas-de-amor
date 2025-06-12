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
}
