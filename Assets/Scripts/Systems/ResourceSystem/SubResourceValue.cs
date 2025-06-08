using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SubResource
{
	public ResourceType resourceType;
	public int subType;
	public SubResource(ResourceType resourceType, int subType)
	{
		this.resourceType = resourceType;
		this.subType = subType;
	}
	
	public override bool Equals(object obj)
	{
		return obj is SubResource other &&
		       resourceType == other.resourceType &&
		       subType == other.subType;
	}
	public override int GetHashCode()
	{
		return (int)resourceType * 397 ^ subType;
	}
}

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
	public SubResource subResource;
	public T resourceValue;
	public SubResourceValue(SubResource subResource, T resourceValue)
	{
		this.subResource = subResource;
		this.resourceValue = resourceValue;
	}
	private SubResourceValue(ResourceType resourceType, int subType, T resourceValue)
	{
		this.subResource = new SubResource(resourceType, subType);
		this.resourceValue = resourceValue;
	}
	
	public SubResourceValue(Enum subType, T resourceValue)
	{
		subResource = new SubResource(GetMainTypeFromSubType(subType), Convert.ToInt32(subType));
		this.resourceValue = resourceValue;
	}
	
	public static Enum GetEnumFromTypeAndIndex(ResourceType mainType, int index)
	{
		if (MappingMainSubType.TryGetValue(mainType, out var enumType))
		{
			if (Enum.IsDefined(enumType, index))
			{
				return (Enum)Enum.ToObject(enumType, index);
			}
			else
			{
				Debug.LogWarning($"Index {index} is not defined in enum {enumType}");
			}
		}
		else
		{
			Debug.LogWarning($"ResourceType {mainType} not found in MappingMainSubType");
		}

		return null;
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
