using System;
using System.Collections.Generic;
using UnityEngine;

public class Ranch : ProductionBuilding
{
    public override void OnUpgraded()
    {
        Debug.Log($"牧场升级到等级 {currentLevel}");
    }
    
    public override void OnDestroyed()
    {
        base.OnDestroyed();
        StopProduction();
        Debug.Log($"牧场被摧毁，位置: {string.Join(" ", positions)}");
    }

    public override void InitialSelfStorage()
    {
        AcceptResources = new HashSet<ResourceConfig>()
        {
            ResourceManager.Instance.GetResourceConfig(ResourceType.Feed, (int)FeedSubType.Feed),
            ResourceManager.Instance.GetResourceConfig(ResourceType.BreedingStock, (int)BreedingStockSubType.Cattle),
            ResourceManager.Instance.GetResourceConfig(ResourceType.BreedingStock, (int)BreedingStockSubType.Sheep),
        };
        inventory = new Inventory(
            new List<ResourceStack>()
            {
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Feed, (int)FeedSubType.Feed), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.BreedingStock, (int)BreedingStockSubType.Cattle), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.BreedingStock, (int)BreedingStockSubType.Sheep), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Livestock, (int)LivestockSubType.Cattle), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Livestock, (int)LivestockSubType.Sheep), 0),
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
            new ConversionRule()
            {
                inputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Feed, (int)FeedSubType.Feed), 2),
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.BreedingStock, (int)BreedingStockSubType.Cattle), 1)
                },
                outputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Livestock, (int)LivestockSubType.Cattle), 1)
                }
            },
            new ConversionRule()
            {
                inputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Feed, (int)FeedSubType.Feed), 2),
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.BreedingStock, (int)BreedingStockSubType.Sheep), 1)
                },
                outputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetResourceConfig(ResourceType.Livestock, (int)LivestockSubType.Sheep), 1)
                }
            }
        };
    }
    
    public void ChangeAnimalType(LivestockSubType newAnimalType)
    {
    }
} 