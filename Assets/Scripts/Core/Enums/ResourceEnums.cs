// 基础资源类型

using System;
using System.Collections.Generic;

public enum ResourceType
{
    Seed,      // 种子
    Crop,      // 作物
    Feed,      // 饲料
    BreedingStock, // 种畜
    Livestock,    // 牲畜
    Coin,      // 金币
    Ticket     // 奖励券
}

// 种子子类型
public enum SeedSubType
{
    Wheat = 0,     // 小麦种子
    Corn = 1       // 玉米种子
}

// 作物子类型
public enum CropSubType
{
    Wheat = 0,     // 小麦
    Corn = 1       // 玉米
}

// 饲料子类型
public enum FeedSubType{
    Feed = 0       // 饲料
}
// 种畜子类型
public enum BreedingStockSubType
{
    Cattle = 0,    // 种牛
    Sheep = 1      // 种羊
}

// 牲畜子类型
public enum LivestockSubType
{
    Cattle = 0,    // 牛
    Sheep = 1      // 羊
} 

// 货币子类型
public enum CoinSubType
{
    Gold = 0,     // 金币
}

// 奖励券子类型
public enum TicketSubType
{
    Ticket = 0,     // 奖励券
}



// 资源子类型工具类
public static class ResourceSubTypeHelper
{
    public static readonly Dictionary<ResourceType, Type> MappingMainSubType =
        new Dictionary<ResourceType, Type>()
        {
            { ResourceType.Seed, typeof(SeedSubType) },
            { ResourceType.Crop, typeof(CropSubType) },
            { ResourceType.Feed, typeof(FeedSubType) },
            { ResourceType.BreedingStock, typeof(BreedingStockSubType) },
            { ResourceType.Livestock, typeof(LivestockSubType) },
            { ResourceType.Coin, typeof(CoinSubType) },
            { ResourceType.Ticket, typeof(TicketSubType) },
        };
    public static readonly Dictionary<Type, ResourceType> MappingSubTypeMain =
        new Dictionary<Type, ResourceType>()
        {
            { typeof(SeedSubType), ResourceType.Seed },
            { typeof(CropSubType), ResourceType.Crop },
            { typeof(FeedSubType), ResourceType.Feed },
            { typeof(BreedingStockSubType), ResourceType.BreedingStock },
            { typeof(LivestockSubType), ResourceType.Livestock },
            { typeof(CoinSubType), ResourceType.Coin },
            { typeof(TicketSubType), ResourceType.Ticket },
        };
    // 将枚举转换为int
    public static int ToInt<T>(T enumValue) where T : System.Enum
    {
        return System.Convert.ToInt32(enumValue);
    }
    
    // 将int转换为枚举
    public static T FromInt<T>(int value) where T : System.Enum
    {
        return (T)System.Enum.ToObject(typeof(T), value);
    }
    
    // 根据ResourceType获取对应的SubType枚举类型
    public static System.Type GetSubTypeEnum(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Seed => typeof(SeedSubType),
            ResourceType.Crop => typeof(CropSubType),
            ResourceType.Feed => typeof(FeedSubType),
            ResourceType.BreedingStock => typeof(BreedingStockSubType),
            ResourceType.Livestock => typeof(LivestockSubType),
            ResourceType.Coin => typeof(CoinSubType),
            ResourceType.Ticket => typeof(TicketSubType),
            _ => throw new System.ArgumentException($"Unknown ResourceType: {resourceType}")
        };
    }
    
    // 验证subType是否对resourceType有效
    public static bool IsValidSubType(ResourceType resourceType, int subType)
    {
        var enumType = GetSubTypeEnum(resourceType);
        return System.Enum.IsDefined(enumType, subType);
    }
    
    // 获取子类型的显示名称
    public static string GetSubTypeName(ResourceType resourceType, int subType)
    {
        var enumType = GetSubTypeEnum(resourceType);
        var enumValue = System.Enum.ToObject(enumType, subType);
        return enumValue.ToString();
    }
}