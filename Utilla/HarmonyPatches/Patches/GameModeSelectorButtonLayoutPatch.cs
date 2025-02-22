using HarmonyLib;

namespace Utilla.HarmonyPatches.Patches;

[HarmonyPatch(typeof(GameModeSelectorButtonLayout))]
public class GameModeSelectorButtonLayoutPatch
{
    [HarmonyPatch("SetupButtons"), HarmonyPrefix]//uh fuck this thing we dont need it
    public static bool SetupButtonsPatch()
    {
        return false;
    }
}