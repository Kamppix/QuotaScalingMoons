using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
    public class MoonPricePatch
    {
        private static void Postfix(TerminalNode __result)
        {
            if (__result.buyRerouteToMoon == -2 || (__result.buyRerouteToMoon != -1 && __result.buyRerouteToMoon != -2))
            {
                __result.itemCost = 0;
            }
        }
    }
}
