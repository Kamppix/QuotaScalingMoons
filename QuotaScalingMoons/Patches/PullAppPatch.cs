using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(LungProp), "DisconnectFromMachinery")]
    public class PullAppPatch
    {
        private static void Prefix(LungProp __instance)
        {
            if (Plugin.BoolConfig["EnableApparaticeValueScaling"].Value)
            {
                __instance.SetScrapValue((int)(__instance.scrapValue * Plugin.GetCurrentValue("ScrapValueMultiplier", true) / Plugin.MinQuotaValues["ScrapValueMultiplier"].Value));
            }
        }
    }
}
