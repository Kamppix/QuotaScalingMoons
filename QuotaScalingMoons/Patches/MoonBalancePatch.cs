using HarmonyLib;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(StartOfRound), "StartGame")]
    public class MoonBalancePatch
    {
        private static void Prefix(StartOfRound __instance)
        {
            SelectableLevel level = __instance.currentLevel;
            if (level.name != "CompanyBuildingLevel")
            {
                level.riskLevel = Plugin.GetCurrentRiskLevel();
                level.factorySizeMultiplier = Plugin.GetCurrentValue("factorySizeMultiplier");
                level.minScrap = (int) Plugin.GetCurrentValue("minScrap");
                level.maxScrap = (int) Plugin.GetCurrentValue("maxScrap");
                level.maxEnemyPowerCount = (int) Plugin.GetCurrentValue("maxEnemyPowerCount");
                level.maxOutsideEnemyPowerCount = (int) Plugin.GetCurrentValue("maxOutsideEnemyPowerCount");
            }
        }
    }
}
