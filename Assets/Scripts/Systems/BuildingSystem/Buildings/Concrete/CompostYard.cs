using System;
using System.Collections.Generic;
using UnityEngine;

public class CompostYard : ProductionBuilding
{
    public override void OnUpgraded()
    {
        Debug.Log($"堆肥场升级到等级 {currentLevel}");
    }
    
    public override void OnDestroyed()
    {
        StopProduction();
        Debug.Log($"堆肥场被摧毁，位置: {string.Join(" ", positions)}");
    }
    
    public override void InitialSelfStorage()
    {
        AcceptResources = new HashSet<ResourceConfig>()
        {
            ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Wheat),
            ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Corn),
        };
        inventory = new Inventory(
            new List<ResourceStack>()
            {
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Wheat), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Corn), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Feed, (int)FeedSubType.Feed), 0),
            },
            Inventory.InventoryAcceptMode.OnlyDefined,
            Inventory.InventoryListFilterMode.AcceptList,
            AcceptResources,
            null,
            Inventory.InventoryOwnerType.Building
        );
    }
    
    protected override void SetupProductionRule()
    {
        base.SetupProductionRule();
        productionRules = new List<ConversionRule>()
        {
            new ConversionRule
            {
                inputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Wheat), 2)
                },
                outputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Feed, (int)FeedSubType.Feed), 3)
                }
            }
        };
    }
} 