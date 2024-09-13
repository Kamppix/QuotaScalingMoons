using HarmonyLib;
using System;

namespace QuotaScalingMoons.Patches
{
    [HarmonyPatch(typeof(RoundManager), "GenerateNewLevelClientRpc")]
    public class MoonBalancePatch
    {
        private const int ARTIFICE = 10;

        private static void Prefix(RoundManager __instance)
        {
            if (Plugin.BoolConfig["EnableMoonBalancing"].Value)
            {
                SelectableLevel level = __instance.currentLevel;
                if (level.name != "CompanyBuildingLevel")
                {
                    level.riskLevel = Plugin.GetCurrentRiskLevel();
                    level.factorySizeMultiplier = Plugin.GetCurrentValue("MapSizeMultiplier");
                    level.minScrap = (int)Plugin.GetCurrentValue("MinScrap");
                    level.maxScrap = Math.Max((int)Plugin.GetCurrentValue("MaxScrap"), level.minScrap);
                    level.maxEnemyPowerCount = (int)Plugin.GetCurrentValue("MaxIndoorPower");
                    level.maxOutsideEnemyPowerCount = (int)Plugin.GetCurrentValue("MaxOutdoorPower");
                    level.enemySpawnChanceThroughoutDay = StartOfRound.Instance.levels[ARTIFICE].enemySpawnChanceThroughoutDay;
                    level.outsideEnemySpawnChanceThroughDay = StartOfRound.Instance.levels[ARTIFICE].outsideEnemySpawnChanceThroughDay;
                }
            }
        }
    }
}
