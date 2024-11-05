using HarmonyLib;

namespace Utilla.HarmonyPatches
{
	[HarmonyPatch(typeof(GorillaTagger), "Start")]
    internal static class PostInitializedPatch
	{
		public static void Postfix() => Events.Instance.TriggerGameInitialized();
    }
}
