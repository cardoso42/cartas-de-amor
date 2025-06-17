using System;
using Microsoft.Extensions.Configuration;

namespace CartasDeAmor.Domain.Configuration;

public class GameSettingsProvider
{
    /// <summary>
    /// Initializes the static game settings from configuration
    /// </summary>
    public static void Initialize(IConfiguration configuration)
    {
        var maxPlayers = configuration.GetValue<int>("GameSettings:MaxPlayers");
        if (maxPlayers > 0)
        {
            GameSettings.MaxPlayers = maxPlayers;
        }
    }
}
