using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
    public class SpawnScrapPatch
    {
        private static void Prefix(RoundManager __instance)
        {
            __instance.scrapValueMultiplier = Plugin.GetCurrentValue("scrapValueMultiplier");
        }
    }
}
