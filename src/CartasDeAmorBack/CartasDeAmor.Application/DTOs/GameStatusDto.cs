using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class PlayerStatus
{
    public string UserEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsProtected { get; set; } = false;
    public int Score { get; set; } = 0;
    public int CardsInHand { get; set; } = 1;
}

public class GameStatusDto
{
    public ICollection<PlayerStatus> Players { get; set; } = [];
    public CardType YourCard { get; set; }
    public int FirstPlayerIndex { get; set; } = 0;
    public bool IsProtected { get; set; } = false;
}
