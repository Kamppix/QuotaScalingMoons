using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(LungProp), "DisconnectFromMachinery")]
    public class PullAppPatch
    {
        private static void Prefix(LungProp __instance)
        {
            if (Plugin.BoolConfig["appPatch"].Value)
            {
                __instance.SetScrapValue((int)(__instance.scrapValue * Plugin.GetCurrentValue("scrapValueMultiplier") / Plugin.MinQuotaValues["scrapValueMultiplier"].Value));
            }
        }
    }
}
