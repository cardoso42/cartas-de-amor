using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class PlayerUpdateDto
{
    public string UserEmail { get; set; } = string.Empty;
    public bool IsProtected { get; set; } = false;
    public List<CardType> HoldingCards { get; set; } = [];
    public ICollection<CardType> PlayedCards { get; set; } = [];
    public int Score { get; set; } = 0;
}
