using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Repositories;

namespace CartasDeAmor.Application.Interfaces;

public interface ICardService
{
    /// <summary>
    /// Retrieves the requirements for a specific card type.
    /// </summary>
    /// <param name="cardType">The type of the card.</param>
    /// <returns>The requirements for the specified card type.</returns>
    Task<CardRequirementsDto> GetCardActionRequirementsAsync(Guid roomId, string currentPlayer, CardType cardType);
}