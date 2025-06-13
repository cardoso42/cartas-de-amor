using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.Interfaces;

public interface IGameService
{
    Task<IList<InitialGameStatusDto>> StartGameAsync(Guid roomId, string hostEmail);

    Task<IList<Player>> GetPlayersAsync(Guid roomId);

    Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail);

    Task<CardActionRequirements[]> GetCardRequirementsAsync(CardType cardType);

    Task<PlayerUpdateDto> DrawCardAsync(Guid roomId, string userEmail);

    Task NextPlayerAsync(Guid roomId);
    
    Task<PlayerUpdateDto> GetPlayerStatusAsync(Guid roomId, string userEmail);
}