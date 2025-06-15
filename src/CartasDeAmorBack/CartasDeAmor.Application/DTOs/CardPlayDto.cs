using CartasDeAmor.Domain.Enums;

public class CardPlayDto
{
    public CardType CardType { get; set; }
    public string? TargetPlayerEmail { get; set; }
    public CardType? TargetCardType { get; set; }
    public CardPlayDto() { }
}