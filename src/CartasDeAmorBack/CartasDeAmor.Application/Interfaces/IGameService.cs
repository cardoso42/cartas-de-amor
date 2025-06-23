using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Entities;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Application.Interfaces;

public interface IGameService
{
    Task<List<SpecialMessage>> StartGameAsync(Guid roomId, string hostEmail);
    Task<List<SpecialMessage>> StartNewRoundAsync(Guid roomId);
    Task<List<SpecialMessage>> DrawCardAsync(Guid roomId, string userEmail);
    Task<List<SpecialMessage>> GetCardActionRequirementsAsync(Guid roomId, string currentPlayer, CardType cardType);
    Task<CardResult> PlayCardAsync(Guid roomId, string userEmail, CardPlayDto cardPlay);
    Task<PublicPlayerUpdateDto> SubmitCardChoiceAsync(Guid roomId, string userEmail, CardType keepCardType, List<CardType> returnCardsType);
    Task<IList<Player>> GetPlayersAsync(Guid roomId);
    Task<string> GetPlayerTurnAsync(Guid roomId);
    Task<bool> IsPlayerTurnAsync(Guid roomId, string userEmail);
    Task<string> NextPlayerAsync(Guid roomId);
    Task<PrivatePlayerUpdateDto> GetPlayerStatusAsync(Guid roomId, string userEmail);
    Task<bool> IsRoundOverAsync(Guid roomId);
    Task<bool> IsGameOverAsync(Guid roomId);
    Task<List<SpecialMessage>> FinishRoundAsync(Guid roomId);
    Task<SpecialMessage> FinishGameAsync(Guid roomdId);
}