using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ResourceStack : ISaveable
{
    [Header("资源数据")]
    public ResourceConfig resourceConfig;
    
    [Header("动态数据")]
    public int amount = 0;
    public int storageLimit = 100;
    public int purchasePrice;
    public int sellPrice;
    
    // 构造函数 - 使用ResourceData
    public ResourceStack(ResourceConfig resourceConfig, int amount)
    {
        this.resourceConfig = resourceConfig;
        this.amount = amount;
        
        if (resourceConfig == null)
        {
            throw new System.ArgumentNullException("resourceData cannot be null");
        }
        
        // 验证子类型是否匹配
        if (!resourceConfig.IsValidSubType())
        {
            throw new System.ArgumentException($"SubType {resourceConfig.subType} is not valid for ResourceType {resourceConfig.type}");
        }
    }
    
    public ResourceStack(ResourceConfig resourceConfig, int amount, int limit)
    {
        this.resourceConfig = resourceConfig;
        this.amount = amount;
        this.storageLimit = limit;
        
        if (resourceConfig == null)
        {
            throw new System.ArgumentNullException("resourceData cannot be null");
        }
        
        // 验证子类型是否匹配
        if (!resourceConfig.IsValidSubType())
        {
            throw new System.ArgumentException($"SubType {resourceConfig.subType} is not valid for ResourceType {resourceConfig.type}");
        }
    }
    
    // 属性访问器，用于向后兼容
    public ResourceType type => resourceConfig?.type ?? ResourceType.Coin;
    public int subType => resourceConfig?.subType ?? 0;
    public string displayName => resourceConfig?.displayName ?? "Unknown";
    public string description => resourceConfig?.description ?? "Unknown";
    public bool canBePurchased => resourceConfig?.canBePurchased ?? false;
    public bool canBeSold => resourceConfig?.canBeSold ?? false;
    public Sprite icon => resourceConfig?.icon;

    // 获取类型安全的子类型
    public T GetSubType<T>() where T : System.Enum
    {
        return resourceConfig != null ? resourceConfig.GetSubType<T>() : default(T);
    }
    
    // 获取子类型的显示名称
    public string GetSubTypeName()
    {
        return resourceConfig != null ? resourceConfig.GetSubTypeName() : "Unknown";
    }
    
    public bool CanStackWith(ResourceStack other) 
    {
        return other != null && 
               other.resourceConfig != null &&
               this.resourceConfig != null &&
               other.resourceConfig.type == this.resourceConfig.type && 
               other.resourceConfig.subType == this.resourceConfig.subType;
    }

    public bool CanAdd(int amount)
    {
        int tempAmount = amount + this.amount;
        return tempAmount <= this.storageLimit && tempAmount >= 0;
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

    public bool IsFull()
    {
        return amount >= this.storageLimit;
    }
    public ResourceStack Clone()
    {
        return new ResourceStack(this.resourceConfig, this.amount);
    }

    #region 便捷的创建方法 - 现在需要ResourceData资产
    // 注意：这些静态方法现在需要预先创建的ResourceData资产
    // 建议在项目中为每种资源类型创建对应的ResourceData ScriptableObject资产
    
    public static ResourceStack CreateFromData(ResourceConfig resourceConfig, int amount)
    {
        return new ResourceStack(resourceConfig, amount);
    }
    
    // 这些方法现在已弃用，需要使用ResourceData资产
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static ResourceStack CreateSeed(SeedSubType seedType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for seeds and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static ResourceStack CreateCrop(CropSubType cropType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for crops and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static ResourceStack CreateBreedingStock(BreedingStockSubType breedingStockType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for breeding stock and use CreateFromData instead");
    }

    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static ResourceStack CreateLivestock(LivestockSubType livestockType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for livestock and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static ResourceStack CreateFeed(FeedSubType feedType, int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for feed and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static ResourceStack CreateCoin(int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for coins and use CreateFromData instead");
    }
    
    [System.Obsolete("Use CreateFromData with ResourceData asset instead")]
    public static ResourceStack CreateTicket(int amount)
    {
        throw new System.NotSupportedException("Create ResourceData asset for tickets and use CreateFromData instead");
    }

    #endregion
    
    public override string ToString()
    {
        return $"{GetSubTypeName()} x{amount}";
    }
    #region 保存与加载
    public GameSaveData GetSaveData()
    {
        return new ResourceStackSaveData(){
            type = type,
            subType = subType,
            amount = amount,
            storageLimit = storageLimit
        };
    }

    public void LoadFromData(GameSaveData data)
    {
        ResourceStackSaveData resourceStackSaveData = data as ResourceStackSaveData;
        // TODO: 从ResourceStackSaveData中加载数据
    }
    #endregion
}