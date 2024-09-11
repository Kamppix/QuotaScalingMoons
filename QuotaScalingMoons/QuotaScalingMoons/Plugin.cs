using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;

namespace QuotaScalingMoons
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        public static Dictionary<String, ConfigEntry<float>> MinQuotaValues = new Dictionary<String, ConfigEntry<float>>();
        public static Dictionary<String, ConfigEntry<float>> HighQuotaValues = new Dictionary<String, ConfigEntry<float>>();
        
        private const float EXP_AVERAGE_VALUE = 31.71354f;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();
            AddConfig();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }

        private void AddConfig()
        {
            MinQuotaValues.Add("profitQuota", Config.Bind(
                "MinQuotaValues",
                "profitQuota",
                130f,
                "Quota")
            );
            MinQuotaValues.Add("factorySizeMultiplier", Config.Bind(
                "MinQuotaValues",
                "factorySizeMultiplier",
                1.0f,
                "Map size multiplier")
            );
            MinQuotaValues.Add("scrapValueMultiplier", Config.Bind(
                "MinQuotaValues",
                "scrapValueMultiplier",
                0.4f,
                "Scrap value multiplier")
            );
            MinQuotaValues.Add("minScrap", Config.Bind(
                "MinQuotaValues",
                "minScrap",
                8f,
                "Min scrap")
            );
            MinQuotaValues.Add("maxScrap", Config.Bind(
                "MinQuotaValues",
                "maxScrap",
                12f,
                "Max scrap")
            );
            /*
            MinQuotaValues.Add("maxTurrets", Config.Bind(
                "MinQuotaValues",
                "maxTurrets",
                7f,
                "Max turrets")
            );
            MinQuotaValues.Add("maxLandmines", Config.Bind(
                "MinQuotaValues",
                "maxLandmines",
                12f,
                "Max landmines")
            );
            MinQuotaValues.Add("maxSpikes", Config.Bind(
                "MinQuotaValues",
                "maxSpikes",
                0f,
                "Max spike traps")
            );
            */
            MinQuotaValues.Add("maxEnemyPowerCount", Config.Bind(
                "MinQuotaValues",
                "maxEnemyPowerCount",
                4f,
                "Max indoor power")
            );
            MinQuotaValues.Add("maxOutsideEnemyPowerCount", Config.Bind(
                "MinQuotaValues",
                "maxOutsideEnemyPowerCount",
                8f,
                "Max outdoor power")
            );



            HighQuotaValues.Add("profitQuota", Config.Bind(
                "HighQuotaValues",
                "profitQuota",
                3536.25f,
                "Quota")
            );
            HighQuotaValues.Add("factorySizeMultiplier", Config.Bind(
                "HighQuotaValues",
                "factorySizeMultiplier",
                1.8f,
                "Map size multiplier")
            );
            HighQuotaValues.Add("scrapValueMultiplier", Config.Bind(
                "HighQuotaValues",
                "scrapValueMultiplier",
                0.74f,
                "Scrap value multiplier")
            );
            HighQuotaValues.Add("minScrap", Config.Bind(
                "HighQuotaValues",
                "minScrap",
                26f,
                "Min scrap")
            );
            HighQuotaValues.Add("maxScrap", Config.Bind(
                "HighQuotaValues",
                "maxScrap",
                31f,
                "Max scrap")
            );
            /*
            HighQuotaValues.Add("maxTurrets", Config.Bind(
                "HighQuotaValues",
                "maxTurrets",
                10f,
                "Max turrets")
            );
            HighQuotaValues.Add("maxLandmines", Config.Bind(
                "HighQuotaValues",
                "maxLandmines",
                35f,
                "Max landmines")
            );
            HighQuotaValues.Add("maxSpikes", Config.Bind(
                "HighQuotaValues",
                "maxSpikes",
                5f,
                "Max spike traps")
            );
            */
            HighQuotaValues.Add("maxEnemyPowerCount", Config.Bind(
                "HighQuotaValues",
                "maxEnemyPowerCount",
                13f,
                "Max indoor power")
            );
            HighQuotaValues.Add("maxOutsideEnemyPowerCount", Config.Bind(
                "HighQuotaValues",
                "maxOutsideEnemyPowerCount",
                13f,
                "Max outdoor power")
            );
        }

        internal static float GetCurrentValue(String key)
        {
            float min = MinQuotaValues[key].Value;
            float add = HighQuotaValues[key].Value - min;
            float progress = (TimeOfDay.Instance.profitQuota - MinQuotaValues["profitQuota"].Value) / (HighQuotaValues["profitQuota"].Value - MinQuotaValues["profitQuota"].Value);
            float result = min + add * progress;

            if (key == "scrapValueMultiplier")
            {
                result /= GetScrapValueDivider();
            }

            return result;
        }

        private static float GetScrapValueDivider()
        {
            SelectableLevel level = StartOfRound.Instance.currentLevel;
            float totalWeight = 0f;
            float averageTotalValue = 0f;

            foreach (SpawnableItemWithRarity item in level.spawnableScrap)
            {
                totalWeight += item.rarity;
            }
            foreach (SpawnableItemWithRarity item in level.spawnableScrap)
            {
                float averageValue = (item.spawnableItem.minValue + item.spawnableItem.maxValue) / 2f * 0.4f;
                averageTotalValue += item.rarity * averageValue / totalWeight;
            }

            return averageTotalValue / EXP_AVERAGE_VALUE;
        }

        internal static string GetCurrentRiskLevel()
        {
            int level = (int) (6 * (TimeOfDay.Instance.profitQuota - MinQuotaValues["profitQuota"].Value) / (HighQuotaValues["profitQuota"].Value - MinQuotaValues["profitQuota"].Value));
            switch (level)
            {
                case 0:
                    return "D";
                case 1:
                    return "C";
                case 2:
                    return "B";
                case 3:
                    return "A";
                default:
                    return "S" + String.Concat(Enumerable.Repeat("+", level - 4));
            }
        }
    }
}
