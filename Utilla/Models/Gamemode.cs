using System;
using System.Linq;
using UnityEngine;

namespace Utilla.Models
{
	/// <summary>
	/// The base gamemode for a gamemode to inherit.
	/// </summary>
	/// <remarks>
	/// None should not be used from an external program.
	/// </remarks>
	public enum BaseGamemode
	{
		/// <summary>
		/// No gamemode, only used for fully custom gamemodes.
		/// </summary>
		None,
		/// <summary>
		/// The regular infection (tag) gamemode.
		/// </summary>
		Infection, 
		/// <summary>
		/// Casual gamemode, no players are infected.
		/// </summary>
		Casual,
		/// <summary>
		/// Hunt gamemode, requires at least 4 players.
		/// </summary>
		Hunt,
        /// <summary>
        /// Paintbrawl gamemode, a gamemode that lets you play a game of paintball with two or more players.
        /// </summary>
        Battle,
		Ambush
	}

	public class Gamemode {
		public const string GamemodePrefix = "MODDED_";

		/// <summary>
		/// The title of the Gamemode visible through the gamemode selector
		/// </summary>
		public string DisplayName { get; }

		/// <summary>
		/// The internal ID of the Gamemode
		/// </summary>
		public string ID { get; }

		/// <summary>
		/// The GamemodeString used in the CustomProperties of the Room
		/// </summary>
		public string GamemodeString { get; }

		/// <summary>
		/// The BaseGamemode being inherited
		/// </summary>
		public BaseGamemode BaseGamemode { get; }
		public Type GameManager { get; }

        public Gamemode(string id, string displayName, BaseGamemode baseGamemode = BaseGamemode.None)
		{
			ID = id + (baseGamemode == BaseGamemode.None || Enum.GetNames(typeof(BaseGamemode)).Any(m => id.Contains(m.ToUpper())) ? "" : baseGamemode.ToString().ToUpper());
			DisplayName = displayName;
			BaseGamemode = baseGamemode;

			GamemodeString = GamemodePrefix + ID.Replace(GamemodePrefix, "");
		}

		public Gamemode(string id, string displayName, Type gameManager)
		{
			this.ID = id;
			this.DisplayName = displayName;
			this.BaseGamemode = BaseGamemode.None;
			this.GameManager = gameManager;

			GamemodeString = GamemodePrefix + ID;
		}

		/// <remarks>This should only be used interally to create base game gamemodes</remarks>
		internal Gamemode(string id, string displayName)
		{
			this.ID = id;
			this.DisplayName = displayName;
			this.BaseGamemode = BaseGamemode.None;

			GamemodeString = ID;
		}

        public static implicit operator ModeSelectButtonInfo(Gamemode gamemode)
		{
			return new ModeSelectButtonInfo()
			{
				Mode = gamemode.ID,
				ModeTitle = gamemode.DisplayName,
				NewMode = false,
				CountdownTo = null
			};
		}

		public static implicit operator Gamemode(ModeSelectButtonInfo modeSelectButtonInfo)
		{
			return new Gamemode(modeSelectButtonInfo.Mode, modeSelectButtonInfo.ModeTitle);
		}
    }
}
