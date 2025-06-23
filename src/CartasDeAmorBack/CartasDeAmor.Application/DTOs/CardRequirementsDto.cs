using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class CardRequirementsDto
{
    public CardType CardType { get; set; }
    public List<CardActionRequirements> Requirements { get; set; } = [];
    public List<string> PossibleTargets { get; set; } = [];
    public List<CardType> PossibleCardTypes { get; set; } = [];

    public CardRequirementsDto() { }
}