using BepInEx;
using System;
using GorillaExtensions;
using UnityEngine;
using Utilla.HarmonyPatches;
using Utilla.Tools;
using Utilla.Utils;
using Utilla.Behaviours;

namespace Utilla
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public void Start()
        {
            Logging.Logger = Logger;
            UtillaPatches.ApplyHarmonyPatches();

            DontDestroyOnLoad(this);
            RoomUtils.RoomCode = RoomUtils.RandomString(6); // Generate a random room code in case we need it
        }

        public static void PostInitialized()
        {
            new GameObject(Constants.Name, typeof(UtillaNetworkController), typeof(GamemodeManager));
        }
    }
}
