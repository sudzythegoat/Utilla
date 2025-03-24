using HarmonyLib;
using Utilla.Behaviours;

namespace Utilla.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(GameModeSelectorButtonLayout))]
    public class GameModeSelectorButtonLayoutPatch
    {
        [HarmonyPatch("OnEnable"), HarmonyPrefix]
        public static bool OnEnablePatch(GameModeSelectorButtonLayout __instance)
        {
            __instance.SetupButtons();
            return false;
        }
        
        [HarmonyPatch(nameof(GameModeSelectorButtonLayout.SetupButtons)), HarmonyPrefix]
        public static void SetupButtonsPrefix(GameModeSelectorButtonLayout __instance)
        {
            NetworkSystem.Instance.OnJoinedRoomEvent -= __instance.SetupButtons;
            SetGameModePatch.PreventSettingMode = true;
        }

        [HarmonyPatch(nameof(GameModeSelectorButtonLayout.SetupButtons)), HarmonyPostfix]
        public static void SetupButtonsPostfix(GameModeSelectorButtonLayout __instance)
        {
            SetGameModePatch.PreventSettingMode = false;
            if (!__instance.GetComponent<UtillaGamemodeSelector>())
            {
                __instance.AddComponent<UtillaGamemodeSelector>();
            }
        }
    }
}