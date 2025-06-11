using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Application.Interfaces;

public interface IGameService
{
    Task<IList<GameStatusDto>> StartGameAsync(Guid roomId, string hostEmail);

    Task<IList<Player>> GetPlayersAsync(Guid roomId);
}