using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public class CardResult
{
    public List<SpecialMessage> SpecialMessages { get; set; } = [];
    public bool ShouldAdvanceTurn { get; set; } = true;

    public CardResult() { }
}