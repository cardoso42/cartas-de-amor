using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.Interfaces;

public interface IGameService
{
    Task StartGameAsync(Guid roomId, string hostEmail);
    Task StartNewRoundAsync(Guid roomId);
    Task DrawCardAsync(Guid roomId, string userEmail);
    Task GetCardActionRequirementsAsync(Guid roomId, string currentPlayer, CardType cardType);
    Task<CardResult> PlayCardAsync(Guid roomId, string userEmail, CardPlayDto cardPlay);
    Task SubmitCardChoiceAsync(Guid roomId, string userEmail, CardType keepCardType, List<CardType> returnCardsType);
    Task<IList<Player>> GetPlayersAsync(Guid roomId);
    Task<string> GetPlayerTurnAsync(Guid roomId);
    Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail);
    Task<string> NextPlayerAsync(Guid roomId);
    Task<PrivatePlayerUpdateDto> GetPlayerStatusAsync(Guid roomId, string userEmail);
    Task<bool> IsRoundOverAsync(Guid roomId);
    Task<bool> IsGameOverAsync(Guid roomId);
    Task FinishRoundAsync(Guid roomId);
	Task VerifyGameValidity(Guid roomId);
    Task FinishGameAsync(Guid roomdId);
    Task<GameStatusDto?> GetCurrentGameStatusAsync(Guid roomId, string userEmail);
}
