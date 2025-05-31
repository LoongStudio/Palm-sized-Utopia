[System.Serializable]
public class Resource
{
    public ResourceType type;
    public int subType;
    public int amount;
    public ResourceData data;
    
    // 构造函数 - 类型安全版本
    public Resource(ResourceType type, System.Enum subTypeEnum, int amount)
    {
        this.type = type;
        this.subType = ResourceSubTypeHelper.ToInt(subTypeEnum);
        this.amount = amount;
        
        // 验证子类型是否匹配
        if (!ResourceSubTypeHelper.IsValidSubType(type, this.subType))
        {
            throw new System.ArgumentException($"SubType {subTypeEnum} is not valid for ResourceType {type}");
        }
    }
    // 构造函数 - int版本（内部使用）
    public Resource(ResourceType type, int subType, int amount) 
    { 
        this.type = type;
        this.subType = subType;
        this.amount = amount;
        
        // 验证子类型是否有效
        if (!ResourceSubTypeHelper.IsValidSubType(type, subType))
        {
            throw new System.ArgumentException($"SubType {subType} is not valid for ResourceType {type}");
        }
    }

    // 获取类型安全的子类型
    public T GetSubType<T>() where T : System.Enum
    {
        return ResourceSubTypeHelper.FromInt<T>(subType);
    }
    // 获取子类型的显示名称
    public string GetSubTypeName()
    {
        return ResourceSubTypeHelper.GetSubTypeName(type, subType);
    }
    
    public bool CanStackWith(Resource other) 
    {
        return other != null && 
               other.type == this.type && 
               other.subType == this.subType;
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

    #region 便捷的创建方法
    public static Resource CreateSeed(SeedSubType seedType, int amount)
    {
        return new Resource(ResourceType.Seed, seedType, amount);
    }
    
    public static Resource CreateCrop(CropSubType cropType, int amount)
    {
        return new Resource(ResourceType.Crop, cropType, amount);
    }
    
    public static Resource CreateBreedingStock(BreedingStockSubType breedingStockType, int amount)
    {
        return new Resource(ResourceType.BreedingStock, breedingStockType, amount);
    }

    public static Resource CreateLivestock(LivestockSubType livestockType, int amount)
    {
        return new Resource(ResourceType.Livestock, livestockType, amount);
    }
    
    public static Resource CreateFeed(FeedSubType feedType, int amount)
    {
        return new Resource(ResourceType.Feed, feedType, amount);
    }
    
    public static Resource CreateCoin(int amount)
    {
        return new Resource(ResourceType.Coin, CoinSubType.Gold, amount);
    }
    
    public static Resource CreateTicket(int amount)
    {
        return new Resource(ResourceType.Ticket, TicketSubType.Ticket, amount);
    }

    #endregion
    
    public override string ToString()
    {
        return $"{GetSubTypeName()} x{amount}";
    }
}