using System;
using System.Collections.Generic;

[Serializable]
public class SubResourceValue<T>
{
	// Seed,      // 种子
	// Crop,      // 作物
	// Feed,      // 饲料
	// BreedingStock, // 种畜
	// Livestock,    // 牲畜
	// Coin,      // 金币
	// Ticket     // 奖励券
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
	public ResourceType resourceType;
	public int subType;
	public T resourceValue;
	private SubResourceValue(ResourceType resourceType, int subType, T resourceValue)
	{
		this.resourceType = resourceType;
		this.subType = subType;
		this.resourceValue = resourceValue;
	}
	
	public SubResourceValue(Enum subType, T resourceValue)
	{
		this.resourceType = GetMainTypeFromSubType(subType);
		this.subType = Convert.ToInt32(subType);
		this.resourceValue = resourceValue;
	}
	
	public static Enum IntToEnum(ResourceType type, int subTypeInt)
	{
		if (!MappingMainSubType.TryGetValue(type, out var enumType))
			throw new ArgumentException($"Unknown ResourceType: {type}");

		return (Enum)Enum.ToObject(enumType, subTypeInt);
	}
	
	/// <summary> 从子类型 Enum 获取 ResourceType </summary>
	public static ResourceType GetMainTypeFromSubType(Enum subType)
	{
		return MappingSubTypeMain[subType.GetType()];
	}

	/// <summary> 从 ResourceType 获取其子类型 Enum 类型 </summary>
	public static Type GetSubTypeEnum(ResourceType type)
	{
		return MappingMainSubType[type];
	}

	/// <summary> 从 ResourceType 和整型 subType 构造 Enum 实例 </summary>
	public static Enum GetSubTypeEnumValue(ResourceType type, int subTypeInt)
	{
		var enumType = GetSubTypeEnum(type);
		return (Enum)Enum.ToObject(enumType, subTypeInt);
	}

	/// <summary> 将子类型 Enum 转为 int 值 </summary>
	public static int GetSubTypeInt(Enum subType)
	{
		return Convert.ToInt32(subType);
	}
}
