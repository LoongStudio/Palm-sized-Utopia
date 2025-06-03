using UnityEngine;

[System.Serializable]
public class Resource
{
    [Header("资源数据")]
    public ResourceData resourceData;
    
    [Header("动态数据")]
    public int amount = 0;
    public int storageLimit = 999;
    public int purchasePrice;
    public int sellPrice;
    
    // 构造函数 - 使用ResourceData
    public Resource(ResourceData resourceData, int amount)
    {
        this.resourceData = resourceData;
        this.amount = amount;
        
        if (resourceData == null)
        {
            throw new System.ArgumentNullException("resourceData cannot be null");
        }
        
        // 验证子类型是否匹配
        if (!resourceData.IsValidSubType())
        {
            throw new System.ArgumentException($"SubType {resourceData.subType} is not valid for ResourceType {resourceData.type}");
        }
    }
    
    // 构造函数 - 类型安全版本（需要先创建ResourceData）
    public Resource(ResourceType type, System.Enum subTypeEnum, int amount)
    {
        // 这个构造函数现在需要ResourceData，建议使用上面的构造函数
        throw new System.NotSupportedException("Please use Resource(ResourceData, int) constructor instead. Create ResourceData asset first.");
    }
    
    // 构造函数 - int版本（需要先创建ResourceData）
    public Resource(ResourceType type, int subType, int amount) 
    { 
        // 这个构造函数现在需要ResourceData，建议使用上面的构造函数
        throw new System.NotSupportedException("Please use Resource(ResourceData, int) constructor instead. Create ResourceData asset first.");
    }

    // 属性访问器，用于向后兼容
    public ResourceType type => resourceData?.type ?? ResourceType.Coin;
    public int subType => resourceData?.subType ?? 0;
    public string displayName => resourceData?.displayName ?? "Unknown";
    public string description => resourceData?.description ?? "Unknown";
    public bool canBePurchased => resourceData?.canBePurchased ?? false;
    public bool canBeSold => resourceData?.canBeSold ?? false;
    public Sprite icon => resourceData?.icon;

    // 获取类型安全的子类型
    public T GetSubType<T>() where T : System.Enum
    {
        return resourceData != null ? resourceData.GetSubType<T>() : default(T);
    }
    
    // 获取子类型的显示名称
    public string GetSubTypeName()
    {
        return resourceData != null ? resourceData.GetSubTypeName() : "Unknown";
    }
    
    public bool CanStackWith(Resource other) 
    {
        return other != null && 
               other.resourceData != null &&
               this.resourceData != null &&
               other.resourceData.type == this.resourceData.type && 
               other.resourceData.subType == this.resourceData.subType;
    }
    
    // 添加资源数量
    public void AddAmount(int amount) 
    {
        this.amount += amount;  
    }
    
    // 移除资源数量
    public bool RemoveAmount(int amount) 
    {
        if (this.amount >= amount)
        {
            this.amount -= amount;
            return true;
        }
        return false;          
    }
    
    // 设置存储上限
    public void SetStorageLimit(int limit)
    {
        this.storageLimit = limit;
    }
    
    // 获取存储上限
    public int GetStorageLimit()
    {
        return this.storageLimit;
    }

    #region 便捷的创建方法 - 现在需要ResourceData资产
    // 注意：这些静态方法现在需要预先创建的ResourceData资产
    // 建议在项目中为每种资源类型创建对应的ResourceData ScriptableObject资产
    
    public static Resource CreateFromData(ResourceData resourceData, int amount)
    {
        return new Resource(resourceData, amount);
    }
    
    // 这些方法现在已弃用，需要使用ResourceData资产
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static Resource CreateSeed(SeedSubType seedType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for seeds and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static Resource CreateCrop(CropSubType cropType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for crops and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static Resource CreateBreedingStock(BreedingStockSubType breedingStockType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for breeding stock and use CreateFromData instead");
    }

    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static Resource CreateLivestock(LivestockSubType livestockType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for livestock and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static Resource CreateFeed(FeedSubType feedType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for feed and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static Resource CreateCoin(int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for coins and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static Resource CreateTicket(int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for tickets and use CreateFromData instead");
    }

    #endregion
    
    public override string ToString()
    {
        return $"{GetSubTypeName()} x{amount}";
    }
}