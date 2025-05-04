using ConfigLib;
using ImGuiNET;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace HoDBlocks.Config
{
    public class ConfigLibCompatibility
    {
        // Keg Settings
        private const string settingKegCapacityLitres = "hodblocks:Config.Setting.KegCapacityLitres";
        private const string settingSpoilRateUntapped = "hodblocks:Config.Setting.SpoilRateUntapped";
        private const string settingSpoilRateTapped = "hodblocks:Config.Setting.SpoilRateTapped";
        private const string settingKegIronHoopDropChance = "hodblocks:Config.Setting.KegIronHoopDropChance";
        private const string settingKegTapDropChance = "hodblocks:Config.Setting.KegTapDropChance";

        // Tun Settings
        private const string settingTunCapacityLitres = "hodblocks:Config.Setting.TunCapacityLitres";
        private const string settingTunSpoilRateMultiplier = "hodblocks:Config.Setting.TunSpoilRateMultiplier";
        
        private const string settingWinchLowerSpeed = "hodblocks:Config.Setting.WinchLowerSpeed";
        private const string settingWinchRaiseSpeed = "hodblocks:Config.Setting.WinchRaiseSpeed";
        
        private const string settingKegDropWithLiquid = "hodblocks:Config.Setting.KegDropWithLiquid";
        private const string settingTunDropWithLiquid = "hodblocks:Config.Setting.TunDropWithLiquid";
        public ConfigLibCompatibility(ICoreClientAPI api)
        {
            if (!api.ModLoader.IsModEnabled("configlib"))
            {
                return;
            }

            Init(api);
        }

        private void Init(ICoreClientAPI api)
        {
            if (!api.ModLoader.IsModEnabled("configlib"))
            {
                return;
            }

            api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig("hodblocks", (id, buttons) => EditConfig(id, buttons, api));
        }

        private void EditConfig(string id, ControlButtons buttons, ICoreClientAPI api)
        {
            if (buttons.Save)
            {
                ModConfig.WriteConfig(api, "HoDBlocksConfig.json", HoDBlocksModSystem.LoadedConfig);
            }
            if (buttons.Reload)
            {
                Config reloadedConfig = ModConfig.ReadConfig<Config>(api, "HoDBlocksConfig.json");
                if (reloadedConfig != null)
                {
                    HoDBlocksModSystem.LoadedConfig = reloadedConfig;
                    api.Logger.Notification("Config reloaded from file.");
                }
                else
                {
                    api.Logger.Warning("Failed to reload config from file.");
                }
            }
            if (buttons.Restore)
            {
                Config restoredConfig = ModConfig.ReadConfig<Config>(api, "HoDBlocksConfig.json");
                if (restoredConfig != null)
                {
                    HoDBlocksModSystem.LoadedConfig = restoredConfig;
                    api.Logger.Notification("Config restored from file.");
                }
                else
                {
                    api.Logger.Warning("No saved config found to restore.");
                }
            }
            if (buttons.Defaults)
            {
                if (api.Side == EnumAppSide.Server)
                {
                    HoDBlocksModSystem.LoadedConfig = new Config();
                    ModConfig.WriteConfig(api, "HoDBlocksConfig.json", HoDBlocksModSystem.LoadedConfig);
                }
                else if (api.Side == EnumAppSide.Client)
                {
                    Config savedConfig = ModConfig.ReadConfig<Config>(api, "HoDBlocksConfig.json");
                    if (savedConfig != null)
                    {
                        HoDBlocksModSystem.LoadedConfig = savedConfig;
                    }
                    else
                    {
                        api.Logger.Warning(
                            "No saved config found when reloading defaults; using current config values.");
                    }
                }
            }
            Edit(api, HoDBlocksModSystem.LoadedConfig, id);
        }
        private void Edit(ICoreClientAPI api, Config config, string id)
        {
            ImGui.TextWrapped("HoDBlocks Settings");
            // Keg Settings
            ImGui.SeparatorText("Keg Settings");

            float kegCapacityLitres = config.KegCapacityLitres;
            ImGui.DragFloat(Lang.Get(settingKegCapacityLitres) + $"##kegCapacityLitres-{id}", ref kegCapacityLitres, 1.0f, 10.0f, 500.0f);
            config.KegCapacityLitres = kegCapacityLitres;

            float spoilRateUntapped = config.SpoilRateUntapped;
            ImGui.DragFloat(Lang.Get(settingSpoilRateUntapped) + $"##spoilRateUntapped-{id}", ref spoilRateUntapped, 0.01f, 0.1f, 1.0f);
            config.SpoilRateUntapped = spoilRateUntapped;

            float spoilRateTapped = config.SpoilRateTapped;
            ImGui.DragFloat(Lang.Get(settingSpoilRateTapped) + $"##spoilRateTapped-{id}", ref spoilRateTapped, 0.01f, 0.1f, 1.0f);
            config.SpoilRateTapped = spoilRateTapped;

            float kegIronHoopDropChance = config.KegIronHoopDropChance;
            ImGui.DragFloat(Lang.Get(settingKegIronHoopDropChance) + $"##kegIronHoopDropChance-{id}", ref kegIronHoopDropChance, 0.01f, 0.0f, 1.0f);
            config.KegIronHoopDropChance = kegIronHoopDropChance;

            float kegTapDropChance = config.KegTapDropChance;
            ImGui.DragFloat(Lang.Get(settingKegTapDropChance) + $"##kegTapDropChance-{id}", ref kegTapDropChance, 0.01f, 0.0f, 1.0f);
            config.KegTapDropChance = kegTapDropChance;


            // Tun Settings
            ImGui.SeparatorText("Tun Settings");

            float tunCapacityLitres = config.TunCapacityLitres;
            ImGui.DragFloat(Lang.Get(settingTunCapacityLitres) + $"##tunCapacityLitres-{id}", ref tunCapacityLitres, 1.0f, 10.0f, 2000.0f);
            config.TunCapacityLitres = tunCapacityLitres;

            float tunSpoilRateMultiplier = config.TunSpoilRateMultiplier;
            ImGui.DragFloat(Lang.Get(settingTunSpoilRateMultiplier) + $"##tunSpoilRateMultiplier-{id}", ref tunSpoilRateMultiplier, 0.01f, 0.1f, 5.0f);
            config.TunSpoilRateMultiplier = tunSpoilRateMultiplier;
            
            ImGui.SeparatorText("Winch Speed");
            
            float winchLowerSpeed = config.WinchLowerSpeed;
            ImGui.DragFloat(Lang.Get(settingWinchLowerSpeed) + $"##winchLowerSpeed-{id}", ref winchLowerSpeed, 0.01f, 0.0f, 5.0f);
            config.WinchLowerSpeed = winchLowerSpeed;

            float winchRaiseSpeed = config.WinchRaiseSpeed;
            ImGui.DragFloat(Lang.Get(settingWinchRaiseSpeed) + $"##winchRaiseSpeed-{id}", ref winchRaiseSpeed, 0.01f, 0.0f, 5.0f);
            config.WinchRaiseSpeed = winchRaiseSpeed;
            
            bool kegDropWithLiquid = config.KegDropWithLiquid;
            ImGui.Checkbox(Lang.Get(settingKegDropWithLiquid) + $"##kegDropWithLiquid-{id}", ref kegDropWithLiquid);
            config.KegDropWithLiquid = kegDropWithLiquid;
            
            bool tunDropWithLiquid = config.TunDropWithLiquid;
            ImGui.Checkbox(Lang.Get(settingTunDropWithLiquid) + $"##tunDropWithLiquid-{id}", ref tunDropWithLiquid);
            config.TunDropWithLiquid = tunDropWithLiquid;
        }
    }
}
