using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.Interfaces;

public interface IGameService
{
    Task<IList<GameStatusDto>> StartGameAsync(Guid roomId, string hostEmail);

    Task<IList<Player>> GetPlayersAsync(Guid roomId);

    Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail);

    Task<CardActionRequirements[]> GetCardRequirementsAsync(CardType cardType);

    Task<GameStatusDto> PlayCardAsync(Guid roomId, string userEmail, CardType cardType);

    Task<GameStatusDto> PlayCardWithInputAsync(Guid roomId, string userEmail, CardType cardType, object[] additionalInputs);
}