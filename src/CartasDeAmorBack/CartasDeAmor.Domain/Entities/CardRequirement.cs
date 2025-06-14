using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public class CardRequirements
{
    public required CardType CardType { get; set; }
    public bool IsTargetRequired { get; set; } = false;
    public bool IsCardTypeRequired { get; set; } = false;
    public bool CanChooseSelf { get; set; } = false;
    public bool CanChooseEqualCardType { get; set; } = false;

}