using HarmonyLib;

namespace Utilla.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(GameModeSelectorButtonLayout), "Start")]
    internal class GameModeSelectorPatch
    {
        public static void Postfix(GameModeSelectorButtonLayout __instance) // postfix. since it's after "Start" the buttons are made, UtillaGamemodeSelector looks for buttons right away.
        {
            if (__instance.GetComponent<UtillaGamemodeSelector>()) return;
            __instance.AddComponent<UtillaGamemodeSelector>();
        }
    }
}
