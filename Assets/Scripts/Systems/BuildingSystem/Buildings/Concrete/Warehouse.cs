using System.Collections.Generic;
using UnityEngine;

public class Warehouse : FunctionalBuilding
{

    public override void InitialSelfStorage()
    {
        AcceptResources = new List<SubResource>() { };
        inventory = new Inventory(
            new List<SubResourceValue<int>> { },
            new List<SubResourceValue<int>>()
            {
                new SubResourceValue<int>(SeedSubType.Wheat, 30),
                new SubResourceValue<int>(SeedSubType.Corn, 30),
                new SubResourceValue<int>(CropSubType.Wheat, 30),
                new SubResourceValue<int>(CropSubType.Corn, 30),
                new SubResourceValue<int>(FeedSubType.Feed, 30),
                new SubResourceValue<int>(CoinSubType.Gold, 30),
                new SubResourceValue<int>(TicketSubType.Ticket, 30),
            });
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