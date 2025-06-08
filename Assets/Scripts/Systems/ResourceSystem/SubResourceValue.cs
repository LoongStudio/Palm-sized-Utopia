using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SubResource
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
	
	public static SubResource CreateFromEnum(Enum subTypeEnum)
	{
		if (subTypeEnum == null)
			throw new ArgumentNullException(nameof(subTypeEnum));

		var enumType = subTypeEnum.GetType();

		if (!MappingSubTypeMain.TryGetValue(enumType, out var mainResourceType))
		{
			throw new ArgumentException($"未找到匹配的 ResourceType，枚举类型: {enumType.Name}");
		}

		int subTypeValue = Convert.ToInt32(subTypeEnum);

		return new SubResource(mainResourceType, subTypeValue);
	}

}

[Serializable]
public class SubResourceValue<T>
{
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
	    if (SubResource.MappingMainSubType.TryGetValue(mainType, out var enumType))
	    {
	        if (Enum.IsDefined(enumType, index))
	        {
	            return (Enum)Enum.ToObject(enumType, index);
	        }
	        else
	        {
	            Debug.LogWarning($"Index {index} is not defined in enum {enumType.Name}");
	            return null;
	        }
	    }
	    else
	    {
	        Debug.LogWarning($"ResourceType {mainType} not found in MappingMainSubType");
	        return null;
	    }
	}

	public static Enum IntToEnum(ResourceType type, int subTypeInt)
	{
	    if (!SubResource.MappingMainSubType.TryGetValue(type, out var enumType))
	        throw new ArgumentException($"Unknown ResourceType: {type}");

	    if (!Enum.IsDefined(enumType, subTypeInt))
	        throw new ArgumentException($"Value {subTypeInt} is not defined in enum {enumType.Name}");

	    return (Enum)Enum.ToObject(enumType, subTypeInt);
	}

	/// <summary> 从子类型 Enum 获取 ResourceType </summary>
	public static ResourceType GetMainTypeFromSubType(Enum subType)
	{
	    var type = subType.GetType();
	    if (!SubResource.MappingSubTypeMain.TryGetValue(type, out var mainType))
	        throw new ArgumentException($"Enum type {type.Name} is not mapped to any ResourceType");

	    return mainType;
	}

	/// <summary> 从 ResourceType 获取其子类型 Enum 类型 </summary>
	public static Type GetSubTypeEnum(ResourceType type)
	{
	    if (!SubResource.MappingMainSubType.TryGetValue(type, out var enumType))
	        throw new ArgumentException($"ResourceType {type} not found in SubResource.MappingMainSubType");

	    return enumType;
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
