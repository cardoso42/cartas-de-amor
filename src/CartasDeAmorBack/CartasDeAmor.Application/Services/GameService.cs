using CartasDeAmor.Domain.Repositories;
using CartasDeAmor.Domain.Enums;
using CartasDeAmor.Domain.Factories;
using CartasDeAmor.Application.DTOs;
using CartasDeAmor.Application.Interfaces;
using CartasDeAmor.Domain.Entities;

namespace CartasDeAmor.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRoomRepository _roomRepository;
    private readonly IPlayerRepository _playerRepository;

    public GameService(IGameRoomRepository roomRepository, IPlayerRepository playerRepository)
    {
        _roomRepository = roomRepository;
        _playerRepository = playerRepository;
    }

    public async Task<IList<GameStatusDto>> StartGameAsync(Guid roomId, string hostEmail)
    {
        List<GameStatusDto> startGameDtos = [];

        // Get the game room
        var game = await _roomRepository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found");

        // Verify the user is the host
        if (game.HostEmail != hostEmail)
        {
            throw new InvalidOperationException("Only the host can start the game");
        }

        // Check if we have enough players (Love Letter requires 2-4 players)
        if (game.Players.Count < 2)
        {
            throw new InvalidOperationException("At least 2 players are required to start the game");
        }

        var playersNames = game.Players.Select(p => p.UserEmail).ToList();

        // Create and shuffle the deck
        var deck = CardFactory.CreateShuffledDeck().Select(c => c.CardType).ToList();

        // Reserve one card for the end of the round
        game.ReservedCard = deck[0];
        deck.RemoveAt(0);

        // Deal one card to each player
        var players = game.Players.ToList();

        var playerStatuses = new List<PlayerStatus>();
        for (int i = 0; i < players.Count; i++)
        {
            players[i].HoldingCard = deck[i];
            players[i].PlayedCards = [];
            players[i].Protected = false;
            await _playerRepository.UpdateAsync(players[i]);

            playerStatuses.Add(new PlayerStatus
            {
                UserEmail = players[i].UserEmail,
                Username = players[i].Username,
                IsProtected = players[i].Protected,
                Score = players[i].Score,
                CardsInHand = 1 // Each player starts with one card
            });
        }

        for (int i = 0; i < players.Count; i++)
        {
            startGameDtos.Add(new GameStatusDto
            {
                Players = playerStatuses,
                YourCard = players[i].HoldingCard ?? default,
                FirstPlayerIndex = 0,
            });
        }

        // Remove dealt cards from deck
        for (int i = 0; i < players.Count; i++)
        {
            deck.RemoveAt(0);
        }

        // Store remaining deck in the game
        game.CardsDeck = deck;

        // Determine first player
        var firstPlayer = players[0];

        // Update the game
        await _roomRepository.UpdateAsync(game);

        return startGameDtos;
    }

    public async Task<IList<Player>> GetPlayersAsync(Guid roomId)
    {
        var game = await _roomRepository.GetByIdAsync(roomId);
        if (game == null)
        {
            throw new InvalidOperationException("Room not found");
        }

        return [.. game.Players];
    }
}
