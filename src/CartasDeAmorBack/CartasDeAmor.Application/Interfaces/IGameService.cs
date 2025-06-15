using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Infrastructure.Migrations;

namespace CartasDeAmor.Application.Interfaces;

public interface IGameService
{
    Task<IList<InitialGameStatusDto>> StartGameAsync(Guid roomId, string hostEmail);
    Task<IList<InitialGameStatusDto>> StartNewRoundAsync(Guid roomId);
    Task<PrivatePlayerUpdateDto> DrawCardAsync(Guid roomId, string userEmail);
    Task<CardRequirementsDto> GetCardActionRequirementsAsync(Guid roomId, string currentPlayer, CardType cardType);
    Task<CardActionResultDto> PlayCardAsync(Guid roomId, string userEmail, CardPlayDto cardPlay);
    Task<IList<Player>> GetPlayersAsync(Guid roomId);
    Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail);
    Task<string> NextPlayerAsync(Guid roomId);
    Task<PrivatePlayerUpdateDto> GetPlayerStatusAsync(Guid roomId, string userEmail);
    Task<string?> GetRoundWinnerAsync(Guid roomId);
    Task<string?> GetGameWinnerAsync(Guid roomdId);
}