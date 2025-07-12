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
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Seed, (int)SeedSubType.Wheat), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Seed, (int)SeedSubType.Corn), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Crop, (int)CropSubType.Wheat), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Crop, (int)CropSubType.Corn), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Feed, (int)FeedSubType.Feed), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Coin, (int)CoinSubType.Gold), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Ticket, (int)TicketSubType.Ticket), 0),
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
        foreach (ResourceStack resource in ResourceManager.Instance.ResourceSettings)
        {
            ResourceManager.Instance.SetResourceLimit(resource.resourceConfig, ResourceManager.Instance.GetResourceLimit(resource.resourceConfig) + capacityIncrease);
        }
        
    }
    
    private void DecreaseStorageCapacity()
    {
        int capacityDecrease = storageCapacity * currentLevel;
        
        // 减少所有资源类型的存储上限
        foreach (ResourceStack resource in ResourceManager.Instance.ResourceSettings)
        {
            ResourceManager.Instance.SetResourceLimit(resource.resourceConfig, ResourceManager.Instance.GetResourceLimit(resource.resourceConfig) - capacityDecrease);
        }
    }
} 