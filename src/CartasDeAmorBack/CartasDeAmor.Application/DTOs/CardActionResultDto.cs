using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Application.DTOs;

public class CardActionResultDto
{
    public required PlayerUpdateDto Invoker { get; set; }
    public required PlayerUpdateDto Target { get; set; }
}
