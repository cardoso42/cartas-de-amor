using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.DTOs;

public class CardActionResultDto
{
    public bool Success { get; set; }
    public PlayerUpdateDto? Invoker { get; set; }
    public PlayerUpdateDto? Target { get; set; }
}
