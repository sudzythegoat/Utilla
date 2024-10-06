using HarmonyLib;

namespace Utilla.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(ModIOLoginTeleporter), "FinishTeleport")]
    public class LoginTeleporterPatch
    {
        public static bool hasVirtualStumpLoaded;

        [HarmonyWrapSafe]
        public static void Postfix(bool success)
        {
            if (success && !hasVirtualStumpLoaded)
            {
                hasVirtualStumpLoaded = true;
                Events.Instance.TriggerForceLoadSelector("VirtualStump");
            }
        }
    }
}
