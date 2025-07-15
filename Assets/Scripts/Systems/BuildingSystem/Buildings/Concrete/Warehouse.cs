using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Warehouse : FunctionalBuilding
{

    private void OnEnable()
    {
        GameEvents.OnResourceBoughtConfirmed += OnResourceBoughtConfirmed;
    }
    private void OnDisable()
    {
        GameEvents.OnResourceBoughtConfirmed -= OnResourceBoughtConfirmed;
    }
    private void OnResourceBoughtConfirmed(ResourceEventArgs args)
    {
        var resourceConfig = ResourceManager.Instance.GetResourceConfig(args.resourceType, args.subType);
        if(inventory != null){
            inventory.AddItem(resourceConfig, args.newAmount);
        }
    }
    public override void InitialSelfStorage()
    {
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