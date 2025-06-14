using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Application.Interfaces;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;
using CartasDeAmor.Domain.Repositories;

namespace CartasDeAmor.Application.Services;

public class CardService : ICardService
{
    private readonly IGameRoomRepository _gameRoomRepository;

    public CardService(IGameRoomRepository gameRoomRepository)
    {
        _gameRoomRepository = gameRoomRepository;
    }

    public async Task<CardRequirementsDto> GetCardActionRequirementsAsync(Guid roomId, string currentPlayer, CardType cardType)
    {
        var gameRoom = await _gameRoomRepository.GetByIdAsync(roomId) ?? throw new ArgumentException("Game room not found.");
        var card = CardFactory.Create(cardType);

        var requirements = card.GetCardActionRequirements();

        if (requirements == null)
        {
            return new CardRequirementsDto
            {
                CardType = cardType,
                Message = "No specific requirements for this card type."
            };
        }

        var requirementsDto = new CardRequirementsDto { CardType = cardType };

        if (requirements.IsTargetRequired)
        {
            requirementsDto.Requirements.Add(CardActionRequirements.SelectPlayer);
            requirementsDto.PossibleTargets = gameRoom.Players
                .Select(p => p.UserEmail)
                .ToList();

            if (!requirements.CanChooseSelf)
            {
                requirementsDto.PossibleTargets.Remove(currentPlayer);
            }
        }

        if (requirements.IsCardTypeRequired)
        {
            requirementsDto.Requirements.Add(CardActionRequirements.SelectCardType);
            requirementsDto.PossibleCardTypes = Enum.GetValues<CardType>().ToList();

            if (!requirements.CanChooseEqualCardType)
            {
                requirementsDto.PossibleCardTypes.Remove(cardType);
            }
        }

        return requirementsDto;
    }
}