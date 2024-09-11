using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine.UIElements.Collections;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(RedLocustBees), "Start")]
    public class SpawnHivePatch
    {
        private static void Postfix(RedLocustBees __instance)
        {
            if (Plugin.BoolConfig["hivePatch"].Value)
            {
                __instance.hive.SetScrapValue((int)(__instance.hive.scrapValue * Plugin.GetCurrentValue("scrapValueMultiplier") / Plugin.MinQuotaValues["scrapValueMultiplier"].Value));
            }
        }
    }
}
