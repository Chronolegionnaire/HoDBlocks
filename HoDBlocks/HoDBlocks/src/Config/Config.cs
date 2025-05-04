using Newtonsoft.Json;
using ProtoBuf;
using Vintagestory.API.Common;

namespace HoDBlocks.Config;

[ProtoContract]
public class Config : IModConfig
{
    // Keg Settings
    [ProtoMember(1)] public float KegCapacityLitres { get; set; }
    [ProtoMember(2)] public float SpoilRateUntapped { get; set; }
    [ProtoMember(3)] public float SpoilRateTapped { get; set; }
    [ProtoMember(4)] public float KegIronHoopDropChance { get; set; }
    [ProtoMember(5)] public float KegTapDropChance { get; set; }

    // Tun Settings
    [ProtoMember(6)] public float TunCapacityLitres { get; set; }
    [ProtoMember(7)] public float TunSpoilRateMultiplier { get; set; }
    
    [ProtoMember(8)] 
    public float WinchLowerSpeed { get; set; }
    
    [ProtoMember(9)] 
    public float WinchRaiseSpeed { get; set; }
    [ProtoMember(10, IsRequired = true)] 
    public bool KegDropWithLiquid { get; set; }
    [ProtoMember(11, IsRequired = true)] 
    public bool TunDropWithLiquid { get; set; }


    public Config()
    {
        // Keg
        KegCapacityLitres = 100.0f;
        SpoilRateUntapped = 0.15f;
        SpoilRateTapped = 0.65f;
        KegIronHoopDropChance = 0.8f;
        KegTapDropChance = 0.9f;

        // Tun
        TunCapacityLitres = 950f;
        TunSpoilRateMultiplier = 1.0f;
        
        WinchLowerSpeed = 0.8f;
        WinchRaiseSpeed = 0.8f;
        
        KegDropWithLiquid = true;
        TunDropWithLiquid = false;
    }
    public Config(ICoreAPI api, Config previousConfig = null)
    {
        // Keg Settings
        KegCapacityLitres = previousConfig?.KegCapacityLitres ?? 100.0f;
        SpoilRateUntapped = previousConfig?.SpoilRateUntapped ?? 0.15f;
        SpoilRateTapped = previousConfig?.SpoilRateTapped ?? 0.65f;
        KegIronHoopDropChance = previousConfig?.KegIronHoopDropChance ?? 0.8f;
        KegTapDropChance = previousConfig?.KegTapDropChance ?? 0.9f;

        // Tun Settings
        TunCapacityLitres = previousConfig?.TunCapacityLitres ?? 950.0f;
        TunSpoilRateMultiplier = previousConfig?.TunSpoilRateMultiplier ?? 1.0f;
        
        WinchLowerSpeed = previousConfig?.WinchLowerSpeed ?? 0.8f;
        WinchRaiseSpeed = previousConfig?.WinchRaiseSpeed ?? 0.8f;
        
        KegDropWithLiquid = previousConfig?.KegDropWithLiquid ?? true;
        TunDropWithLiquid = previousConfig?.TunDropWithLiquid ?? false;
    }
}
