using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(LungProp), "DisconnectFromMachinery")]
    public class PullAppPatch
    {
        private static void Prefix(LungProp __instance)
        {
            __instance.SetScrapValue((int) (__instance.scrapValue * Plugin.GetCurrentValue("scrapValueMultiplier") / Plugin.MinQuotaValues["scrapValueMultiplier"].Value));
        }
    }
}
