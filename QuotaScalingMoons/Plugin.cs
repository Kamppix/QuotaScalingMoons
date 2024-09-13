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
        private static Dictionary<String, ConfigEntry<float>> FirstQuotaValues = new Dictionary<String, ConfigEntry<float>>();
        private static Dictionary<String, ConfigEntry<float>> SecondQuotaValues = new Dictionary<String, ConfigEntry<float>>();
        private static Dictionary<String, ConfigEntry<float>> ThirdQuotaValues = new Dictionary<String, ConfigEntry<float>>();

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

        internal static float GetCurrentValue(String key, bool ignoreAvgScrapValue = false)
        {
            Dictionary<String, ConfigEntry<float>> MinQuota;
            Dictionary<String, ConfigEntry<float>> MaxQuota;

            if (BoolConfig["EnableThirdQuota"].Value && TimeOfDay.Instance.profitQuota > SecondQuotaValues["TargetQuota"].Value)
            {
                MinQuota = SecondQuotaValues;
                MaxQuota = ThirdQuotaValues;
            }
            else
            {
                MinQuota = FirstQuotaValues;
                MaxQuota = SecondQuotaValues;
            }

            float min = MinQuota[key].Value;
            float add = MaxQuota[key].Value - min;
            float progress = (TimeOfDay.Instance.profitQuota - MinQuota["TargetQuota"].Value) / (MaxQuota["TargetQuota"].Value - MinQuota["TargetQuota"].Value);
            float result = min + add * progress;

            if ((BoolConfig.ContainsKey("Limit" + key) && BoolConfig["Limit" + key].Value)
                || ((key == "MinScrap" || key == "MaxScrap") && BoolConfig["LimitScrapAmount"].Value))
            {
                result = Math.Min(result, SecondQuotaValues[key].Value);
            }

            if (key == "ScrapValueMultiplier" && !ignoreAvgScrapValue && BoolConfig["EnableMoonBalancing"].Value)
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
            int level = (int) (6 * (TimeOfDay.Instance.profitQuota - FirstQuotaValues["TargetQuota"].Value) / (SecondQuotaValues["TargetQuota"].Value - FirstQuotaValues["TargetQuota"].Value));
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


            FirstQuotaValues.Add("TargetQuota", Config.Bind(
                "First Quota",
                "TargetQuota",
                130f)
            );
            FirstQuotaValues.Add("MapSizeMultiplier", Config.Bind(
                "First Quota",
                "MapSizeMultiplier",
                1.0f)
            );
            FirstQuotaValues.Add("ScrapValueMultiplier", Config.Bind(
                "First Quota",
                "ScrapValueMultiplier",
                0.4f)
            );
            FirstQuotaValues.Add("MinScrap", Config.Bind(
                "First Quota",
                "MinScrap",
                8f)
            );
            FirstQuotaValues.Add("MaxScrap", Config.Bind(
                "First Quota",
                "MaxScrap",
                12f)
            );
            FirstQuotaValues.Add("MaxIndoorPower", Config.Bind(
                "First Quota",
                "MaxIndoorEnemyPower",
                4f)
            );
            FirstQuotaValues.Add("MaxOutdoorPower", Config.Bind(
                "First Quota",
                "MaxOutdoorEnemyPower",
                8f)
            );


            SecondQuotaValues.Add("TargetQuota", Config.Bind(
                "Second Quota",
                "TargetQuota",
                3536.25f)
            );
            SecondQuotaValues.Add("MapSizeMultiplier", Config.Bind(
                "Second Quota",
                "MapSizeMultiplier",
                1.8f)
            );
            SecondQuotaValues.Add("ScrapValueMultiplier", Config.Bind(
                "Second Quota",
                "ScrapValueMultiplier",
                0.74f)
            );
            SecondQuotaValues.Add("MinScrap", Config.Bind(
                "Second Quota",
                "MinScrap",
                26f)
            );
            SecondQuotaValues.Add("MaxScrap", Config.Bind(
                "Second Quota",
                "MaxScrap",
                31f)
            );
            SecondQuotaValues.Add("MaxIndoorPower", Config.Bind(
                "Second Quota",
                "MaxIndoorEnemyPower",
                13f)
            );
            SecondQuotaValues.Add("MaxOutdoorPower", Config.Bind(
                "Second Quota",
                "MaxOutdoorEnemyPower",
                13f)
            );
            BoolConfig.Add("LimitMapSizeMultiplier", Config.Bind(
                "Second Quota",
                "LimitMapSize",
                false,
                "Limits MapSizeMultiplier to its second quota value instead of scaling infinitely.")
            );
            BoolConfig.Add("LimitScrapValueMultiplier", Config.Bind(
                "Second Quota",
                "LimitScrapValue",
                false,
                "Limits ScrapValueMultiplier to its second quota value instead of scaling infinitely.")
            );
            BoolConfig.Add("LimitScrapAmount", Config.Bind(
                "Second Quota",
                "LimitScrapAmount",
                false,
                "Limits MinScrap and MaxScrap to their second quota values instead of scaling infinitely.")
            );
            BoolConfig.Add("LimitMaxIndoorPower", Config.Bind(
                "Second Quota",
                "LimitMaxIndoorEnemyPower",
                false,
                "Limits MaxIndoorEnemyPower to its second quota value instead of scaling infinitely.")
            );
            BoolConfig.Add("LimitMaxOutdoorPower", Config.Bind(
                "Second Quota",
                "LimitMaxOutdoorEnemyPower",
                false,
                "Limits MaxOutdoorEnemyPower to its second quota value instead of scaling infinitely.")
            );


            BoolConfig.Add("EnableThirdQuota", Config.Bind(
                "Third Quota",
                "EnableThirdQuota",
                false,
                "Enables the optional third quota to scale towards after the second quota.")
            );
            ThirdQuotaValues.Add("TargetQuota", Config.Bind(
                "Third Quota",
                "TargetQuota",
                6942.5f)
            );
            ThirdQuotaValues.Add("MapSizeMultiplier", Config.Bind(
                "Third Quota",
                "MapSizeMultiplier",
                2.6f)
            );
            ThirdQuotaValues.Add("ScrapValueMultiplier", Config.Bind(
                "Third Quota",
                "ScrapValueMultiplier",
                1.08f)
            );
            ThirdQuotaValues.Add("MinScrap", Config.Bind(
                "Third Quota",
                "MinScrap",
                44f)
            );
            ThirdQuotaValues.Add("MaxScrap", Config.Bind(
                "Third Quota",
                "MaxScrap",
                50f)
            );
            ThirdQuotaValues.Add("MaxIndoorPower", Config.Bind(
                "Third Quota",
                "MaxIndoorEnemyPower",
                22f)
            );
            ThirdQuotaValues.Add("MaxOutdoorPower", Config.Bind(
                "Third Quota",
                "MaxOutdoorEnemyPower",
                18f)
            );
        }
    }
}
