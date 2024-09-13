using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(RedLocustBees), "Start")]
    public class SpawnHivePatch
    {
        private static void Postfix(RedLocustBees __instance)
        {
            if (Plugin.BoolConfig["EnableHiveValueScaling"].Value)
            {
                __instance.hive.SetScrapValue((int)(__instance.hive.scrapValue * Plugin.GetCurrentValue("ScrapValueMultiplier", true) / 0.4f));
            }
        }
    }
}
