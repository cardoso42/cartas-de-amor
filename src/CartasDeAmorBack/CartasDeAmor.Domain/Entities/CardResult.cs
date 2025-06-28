using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Events;

namespace CartasDeAmor.Domain.Entities;

public class CardResult
{
    public bool ShouldAdvanceTurn { get; set; } = true;
    public List<GameEvent> Events { get; set; } = [];

    public CardResult() { }
}