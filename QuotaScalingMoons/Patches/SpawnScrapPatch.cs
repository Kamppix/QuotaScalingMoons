using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
    public class SpawnScrapPatch
    {
        private static void Prefix(RoundManager __instance)
        {
            if (Plugin.BoolConfig["EnableScrapValueScaling"].Value)
            {
                __instance.scrapValueMultiplier = Plugin.GetCurrentValue("ScrapValueMultiplier");
            }
        }
    }
}
