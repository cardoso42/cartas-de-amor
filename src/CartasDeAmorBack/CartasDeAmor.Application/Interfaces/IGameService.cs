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
    Task<PublicPlayerUpdateDto> SubmitCardChoiceAsync(Guid roomId, string userEmail, CardType keepCardType, List<CardType> returnCardsType);
    Task<IList<Player>> GetPlayersAsync(Guid roomId);
    Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail);
    Task<string> NextPlayerAsync(Guid roomId);
    Task<PrivatePlayerUpdateDto> GetPlayerStatusAsync(Guid roomId, string userEmail);
    Task<bool> IsRoundOverAsync(Guid roomId);
    Task<bool> IsGameOverAsync(Guid roomId);
    Task<IList<string>> FinishRoundAsync(Guid roomId);
    Task<IList<string>> AddBonusPointsAsync(Guid roomId);
    Task<IList<string>> FinishGameAsync(Guid roomdId);
}