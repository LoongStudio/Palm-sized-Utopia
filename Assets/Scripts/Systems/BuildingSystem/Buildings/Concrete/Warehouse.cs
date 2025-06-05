using UnityEngine;

public class Warehouse : Building
{
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