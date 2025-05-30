﻿
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;

namespace HoDBlocks.Config;

public class InitConfig
{
    private string configFilename = "HoDBlocksConfig.json";
    public void LoadConfig(ICoreAPI api)
    {
        var savedConfig = LoadConfigFromFile(api);
        if (savedConfig == null)
        {
            HoDBlocksModSystem.LoadedConfig = new Config();
            SaveConfig(api);
        }
        else
        {
            HoDBlocksModSystem.LoadedConfig = savedConfig;
        }
    }
    private Config LoadConfigFromFile(ICoreAPI api)
    {
        var jsonObj = api.LoadModConfig(configFilename);
        if (jsonObj == null)
        {
            return null;
        }
        var existingJson = JObject.Parse(jsonObj.Token.ToString());
        var configType = typeof(Config);
        var properties = configType.GetProperties();
        var defaultConfig = new Config();
        bool needsSave = false;
    
        foreach (var prop in properties)
        {
            string pascalCaseName = prop.Name;
            string camelCaseName = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
            var hasValue = false;
            JToken value = null;
        
            if (existingJson.ContainsKey(pascalCaseName))
            {
                value = existingJson[pascalCaseName];
                hasValue = value != null && value.Type != JTokenType.Null;
            }
            else if (existingJson.ContainsKey(camelCaseName))
            {
                value = existingJson[camelCaseName];
                hasValue = value != null && value.Type != JTokenType.Null;
            }

            if (!hasValue)
            {
                var defaultValue = prop.GetValue(defaultConfig);
                existingJson[pascalCaseName] = JToken.FromObject(defaultValue);
                needsSave = true;
            }
        }

        var settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Include
        };
    
        var config = JsonConvert.DeserializeObject<Config>(existingJson.ToString(), settings);
    
        if (needsSave)
        {
            SaveConfig(api, config);
        }
    
        return config;
    }

    private void SaveConfig(ICoreAPI api, Config config = null)
    {
        if (config == null)
        {
            config = HoDBlocksModSystem.LoadedConfig;
        }

        if (config == null)
        {
            return;
        }

        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
        };
        var configJson = JsonConvert.SerializeObject(config, jsonSettings);
        ModConfig.WriteConfig(api, configFilename, config);
    }
}