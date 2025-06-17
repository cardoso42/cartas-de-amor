using System;

namespace CartasDeAmor.Domain.Configuration;

public static class GameSettings
{
    /// <summary>
    /// The maximum number of players allowed in a game room
    /// </summary>
    public static int MaxPlayers { get; set; } = 4; // Default value
}
