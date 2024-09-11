using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuotaScalingMoons
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        public static Dictionary<String, ConfigEntry<bool>> BoolConfig = new Dictionary<String, ConfigEntry<bool>>();
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
            BoolConfig.Add("moonBalancePatch", Config.Bind(
                "Toggles",
                "EnableMoonBalancing",
                true)
            );
            BoolConfig.Add("moonPricePatch", Config.Bind(
                "Toggles",
                "EnableFreeMoons",
                true)
            );
            BoolConfig.Add("scrapPatch", Config.Bind(
                "Toggles",
                "EnableScrapValueAdjust",
                true)
            );
            BoolConfig.Add("appPatch", Config.Bind(
                "Toggles",
                "EnableApparaticeValueAdjust",
                false)
            );
            BoolConfig.Add("hivePatch", Config.Bind(
                "Toggles",
                "EnableHiveValueAdjust",
                true)
            );


            MinQuotaValues.Add("profitQuota", Config.Bind(
                "First Quota Values",
                "TargetQuota",
                130f)
            );
            MinQuotaValues.Add("factorySizeMultiplier", Config.Bind(
                "First Quota Values",
                "MapSizeMultiplier",
                1.0f)
            );
            MinQuotaValues.Add("scrapValueMultiplier", Config.Bind(
                "First Quota Values",
                "ScrapValueMultiplier",
                0.4f)
            );
            MinQuotaValues.Add("minScrap", Config.Bind(
                "First Quota Values",
                "MinScrap",
                8f)
            );
            MinQuotaValues.Add("maxScrap", Config.Bind(
                "First Quota Values",
                "MaxScrap",
                12f)
            );
            MinQuotaValues.Add("maxEnemyPowerCount", Config.Bind(
                "First Quota Values",
                "MaxIndoorPower",
                4f)
            );
            MinQuotaValues.Add("maxOutsideEnemyPowerCount", Config.Bind(
                "First Quota Values",
                "MaxOutdoorPower",
                8f)
            );


            HighQuotaValues.Add("profitQuota", Config.Bind(
                "High Quota Values",
                "TargetQuota",
                3536.25f)
            );
            HighQuotaValues.Add("factorySizeMultiplier", Config.Bind(
                "High Quota Values",
                "MapSizeMultiplier",
                1.8f)
            );
            HighQuotaValues.Add("scrapValueMultiplier", Config.Bind(
                "High Quota Values",
                "ScrapValueMultiplier",
                0.74f)
            );
            HighQuotaValues.Add("minScrap", Config.Bind(
                "High Quota Values",
                "MinScrap",
                26f)
            );
            HighQuotaValues.Add("maxScrap", Config.Bind(
                "High Quota Values",
                "MaxScrap",
                31f)
            );
            HighQuotaValues.Add("maxEnemyPowerCount", Config.Bind(
                "High Quota Values",
                "MaxIndoorPower",
                13f)
            );
            HighQuotaValues.Add("maxOutsideEnemyPowerCount", Config.Bind(
                "High Quota Values",
                "MaxOutdoorPower",
                13f)
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
