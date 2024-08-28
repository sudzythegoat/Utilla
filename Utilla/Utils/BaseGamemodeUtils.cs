using GorillaGameModes;
using System;
using Utilla.Models;

namespace Utilla.Utils
{
    public static class BaseGamemodeUtils
    {
        public static string GetName(BaseGamemode gamemode)
        {
            return gamemode.ToString();
        }

        public static string GetShortName(BaseGamemode gamemode)
        {
            return gamemode switch
            {
                BaseGamemode.Paintbrawl => "BATTLE",
                _ => GetName(gamemode)
            };
        }

        public static GameModeType? GetGameModeType(BaseGamemode gamemode)
        {
            if (Enum.TryParse(GetName(gamemode), out GameModeType result))
            {
                return result;
            }
            return null;
        }
    }
}
