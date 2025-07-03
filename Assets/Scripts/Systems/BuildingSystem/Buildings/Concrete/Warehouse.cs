using System.Collections.Generic;
using UnityEngine;

public class Warehouse : FunctionalBuilding
{

    public override void InitialSelfStorage()
    {
        AcceptResources = new HashSet<ResourceConfig>() { };
        inventory = new Inventory(
            new List<ResourceStack>()
            {
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Seed, (int)SeedSubType.Wheat), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Seed, (int)SeedSubType.Corn), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Wheat), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Corn), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Feed, (int)FeedSubType.Feed), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Coin, (int)CoinSubType.Gold), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Ticket, (int)TicketSubType.Ticket), 0),
            },
            Inventory.InventoryAcceptMode.OnlyDefined,
            Inventory.InventoryListFilterMode.AcceptList,
            AcceptResources,
            null,
            Inventory.InventoryOwnerType.Building
        );
    }
    [Header("仓库专属")]
    public int storageCapacity = 500;
    
    public new void OnTryBuilt()
    {
        status = BuildingStatus.Active;
        IncreaseStorageCapacity();
        Debug.Log($"仓库建造完成，位置: {string.Join(" ", positions)}，容量: {storageCapacity}");
    }
    
    public override void OnUpgraded()
    {
        IncreaseStorageCapacity();
        Debug.Log($"仓库升级到等级 {currentLevel}，新容量: {storageCapacity}");
    }
    
    public override void OnDestroyed()
    {
        base.OnDestroyed();
        DecreaseStorageCapacity();
        Debug.Log($"仓库被摧毁，位置: {string.Join(" ", positions)}");
    }
    
    private void IncreaseStorageCapacity()
    {
        int capacityIncrease = storageCapacity * currentLevel;
        
        // 增加所有资源类型的存储上限
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int currentLimit = ResourceManager.Instance.GetStorageLimit(type);
            ResourceManager.Instance.SetStorageLimit(type, currentLimit + capacityIncrease);
        }
    }
    
    private void DecreaseStorageCapacity()
    {
        int capacityDecrease = storageCapacity * currentLevel;
        
        // 减少所有资源类型的存储上限
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int currentLimit = ResourceManager.Instance.GetStorageLimit(type);
            int newLimit = Mathf.Max(100, currentLimit - capacityDecrease); // 最小保留100容量
            ResourceManager.Instance.SetStorageLimit(type, newLimit);
        }
    }
} 