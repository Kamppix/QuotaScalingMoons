using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    public class ScrapValuePatch
    {
        [HarmonyPatch("SpawnScrapInLevel")]
        [HarmonyPrefix]
        private static void IncreaseScrapValueMultiplier(RoundManager __instance)
        {
            __instance.scrapValueMultiplier = 10f;
        }
    }
}
