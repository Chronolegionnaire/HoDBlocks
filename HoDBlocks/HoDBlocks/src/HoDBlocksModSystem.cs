using System;
using HoDBlocks.Config;
using HoDBlocks.Keg;
using HoDBlocks.Winch;
using Newtonsoft.Json;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace HoDBlocks;

public class HoDBlocksModSystem : ModSystem
{
    private ICoreServerAPI _serverApi;
    private ICoreClientAPI _clientApi;
    public static Config.Config LoadedConfig { get; set; }
    private ConfigLibCompatibility _configLibCompatibility;
    
    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        var initConfig = new InitConfig();
        initConfig.LoadConfig(api);
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.RegisterBlockClass("HoDBlocks.BlockKeg", typeof(BlockKeg));
        api.RegisterBlockEntityClass("HoDBlocks.BlockEntityKeg", typeof(BlockEntityKeg));
        api.RegisterItemClass("HoDBlocks.ItemKegTap", typeof(ItemKegTap));
        api.RegisterBlockClass("HoDBlocks.BlockTun", typeof(BlockTun));
        api.RegisterBlockEntityClass("HoDBlocks.BlockEntityTun", typeof(BlockEntityTun));
        api.RegisterBlockClass("HoDBlocks.BlockWinch", typeof(BlockWinch));
        api.RegisterBlockEntityClass("HoDBlocks.BlockEntityWinch", typeof(BlockEntityWinch));
        if (api.Side == EnumAppSide.Server)
        {
            InitializeServer(api as ICoreServerAPI);
        }
        else if (api.Side == EnumAppSide.Client)
        {
            InitializeClient(api as ICoreClientAPI);
        }
    }
    
    private void InitializeServer(ICoreServerAPI api)
    {
        _serverApi = api;
        base.StartServerSide(api);
        string configJson = JsonConvert.SerializeObject(LoadedConfig, Formatting.Indented);
        byte[] configBytes = System.Text.Encoding.UTF8.GetBytes(configJson);
        string base64Config = Convert.ToBase64String(configBytes);
        api.World.Config.SetString("HoDBlocksConfig", base64Config);
    }
    private void InitializeClient(ICoreClientAPI api)
    {
        _clientApi = api;
        base.StartClientSide(api);
        string base64Config = api.World.Config.GetString("HoDBlocksConfig", "");
        if (!string.IsNullOrWhiteSpace(base64Config))
        {
            try
            {
                byte[] configBytes = Convert.FromBase64String(base64Config);
                string configJson = System.Text.Encoding.UTF8.GetString(configBytes);
                LoadedConfig = JsonConvert.DeserializeObject<Config.Config>(configJson);
            }
            catch (Exception ex)
            {
                api.Logger.Error("Failed to deserialize HHoDBlocks config: " + ex);
                LoadedConfig = new Config.Config();
            }
        }
        else
        {
            api.Logger.Warning("HoDBlocks config not found in world config; using defaults.");
            LoadedConfig = new Config.Config();
        }
        _configLibCompatibility = new ConfigLibCompatibility(api);
    }
}