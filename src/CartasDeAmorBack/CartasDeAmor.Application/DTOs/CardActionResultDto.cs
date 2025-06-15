using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class CardActionResultDto
{
    public CardActionResults Result { get; set; } = CardActionResults.None;
    public CardType CardType { get; set; }
    public PublicPlayerUpdateDto? Invoker { get; set; }
    public PublicPlayerUpdateDto? Target { get; set; }
}
