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

        internal static float GetCurrentValue(String key, bool ignoreAverageValue = false)
        {
            float min = MinQuotaValues[key].Value;
            float add = HighQuotaValues[key].Value - min;
            float progress = (TimeOfDay.Instance.profitQuota - MinQuotaValues["TargetQuota"].Value) / (HighQuotaValues["TargetQuota"].Value - MinQuotaValues["TargetQuota"].Value);
            float result = min + add * progress;

            if ((BoolConfig.ContainsKey("Limit" + key) && BoolConfig["Limit" + key].Value)
                || ((key == "MinScrap" || key == "MaxScrap") && BoolConfig["LimitScrapAmount"].Value))
            {
                result = Math.Min(result, HighQuotaValues[key].Value);
            }

            if (key == "ScrapValueMultiplier" && !ignoreAverageValue && BoolConfig["EnableMoonBalancing"].Value)
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
            int level = (int) (6 * (TimeOfDay.Instance.profitQuota - MinQuotaValues["TargetQuota"].Value) / (HighQuotaValues["TargetQuota"].Value - MinQuotaValues["TargetQuota"].Value));
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

        private void AddConfig()
        {
            BoolConfig.Add("EnableFreeMoons", Config.Bind(
                "Toggles",
                "EnableFreeMoons",
                true,
                "Whether or not all moons should be free to route to")
            );
            BoolConfig.Add("EnableMoonBalancing", Config.Bind(
                "Toggles",
                "EnableMoonBalancing",
                true,
                "Whether or not the properties of different moons should be balanced")
            );
            BoolConfig.Add("EnableScrapValueScaling", Config.Bind(
                "Toggles",
                "EnableScrapValueScaling",
                true,
                "Whether or not scrap value should be scaled based on the profit quota")
            );
            BoolConfig.Add("EnableApparaticeValueScaling", Config.Bind(
                "Toggles",
                "EnableApparaticeValueScaling",
                true,
                "Whether or not apparatice value should be scaled based on the profit quota")
            );
            BoolConfig.Add("EnableHiveValueScaling", Config.Bind(
                "Toggles",
                "EnableHiveValueScaling",
                true,
                "Whether or not beehive value should be scaled based on the profit quota")
            );


            MinQuotaValues.Add("TargetQuota", Config.Bind(
                "First Quota Values",
                "TargetQuota",
                130f)
            );
            MinQuotaValues.Add("MapSizeMultiplier", Config.Bind(
                "First Quota Values",
                "MapSizeMultiplier",
                1.0f)
            );
            MinQuotaValues.Add("ScrapValueMultiplier", Config.Bind(
                "First Quota Values",
                "ScrapValueMultiplier",
                0.4f)
            );
            MinQuotaValues.Add("MinScrap", Config.Bind(
                "First Quota Values",
                "MinScrap",
                8f)
            );
            MinQuotaValues.Add("MaxScrap", Config.Bind(
                "First Quota Values",
                "MaxScrap",
                12f)
            );
            MinQuotaValues.Add("MaxIndoorPower", Config.Bind(
                "First Quota Values",
                "MaxIndoorEnemyPower",
                4f)
            );
            MinQuotaValues.Add("MaxOutdoorPower", Config.Bind(
                "First Quota Values",
                "MaxOutdoorEnemyPower",
                8f)
            );


            HighQuotaValues.Add("TargetQuota", Config.Bind(
                "High Quota Values",
                "TargetQuota",
                3536.25f)
            );
            HighQuotaValues.Add("MapSizeMultiplier", Config.Bind(
                "High Quota Values",
                "MapSizeMultiplier",
                1.8f)
            );
            HighQuotaValues.Add("ScrapValueMultiplier", Config.Bind(
                "High Quota Values",
                "ScrapValueMultiplier",
                0.74f)
            );
            HighQuotaValues.Add("MinScrap", Config.Bind(
                "High Quota Values",
                "MinScrap",
                26f)
            );
            HighQuotaValues.Add("MaxScrap", Config.Bind(
                "High Quota Values",
                "MaxScrap",
                31f)
            );
            HighQuotaValues.Add("MaxIndoorPower", Config.Bind(
                "High Quota Values",
                "MaxIndoorEnemyPower",
                13f)
            );
            HighQuotaValues.Add("MaxOutdoorPower", Config.Bind(
                "High Quota Values",
                "MaxOutdoorEnemyPower",
                13f)
            );


            BoolConfig.Add("LimitMapSizeMultiplier", Config.Bind(
                "Limiters",
                "LimitMapSize",
                false,
                "Limits MapSizeMultiplier to its high quota value instead of scaling infinitely")
            );
            BoolConfig.Add("LimitScrapValueMultiplier", Config.Bind(
                "Limiters",
                "LimitScrapValue",
                false,
                "Limits ScrapValueMultiplier to its high quota value instead of scaling infinitely")
            );
            BoolConfig.Add("LimitScrapAmount", Config.Bind(
                "Limiters",
                "LimitScrapAmount",
                false,
                "Limits MinScrap and MaxScrap to their high quota values instead of scaling infinitely")
            );
            BoolConfig.Add("LimitMaxIndoorPower", Config.Bind(
                "Limiters",
                "LimitMaxIndoorEnemyPower",
                false,
                "Limits MaxIndoorEnemyPower to its high quota value instead of scaling infinitely")
            );
            BoolConfig.Add("LimitMaxOutdoorPower", Config.Bind(
                "Limiters",
                "LimitMaxOutdoorEnemyPower",
                false,
                "Limits MaxOutdoorEnemyPower to its high quota value instead of scaling infinitely")
            );
        }
    }
}
