using System;
using BepInEx;
using Utilla.HarmonyPatches;
using Utilla.Utils;
using UnityEngine;
using Utilla.Tools;

namespace Utilla
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private UtillaNetworkController _networkController;

        public void Start()
        {
            Logging.Logger = Logger;

            DontDestroyOnLoad(this);
            RoomUtils.RoomCode = RoomUtils.RandomString(6); // Generate a random room code in case we need it

            _networkController = gameObject.AddComponent<UtillaNetworkController>();

            Events.GameInitialized += PostInitialized;

            UtillaPatches.ApplyHarmonyPatches();
        }

        public void PostInitialized(object sender, EventArgs e)
		{
            Logging.Info("Game initialized");

            GameObject gameModeManagerObject = new(typeof(GamemodeManager).FullName, typeof(GamemodeManager));
            _networkController.gameModeManager = gameModeManagerObject.GetComponent<GamemodeManager>();
		}
    }
}
