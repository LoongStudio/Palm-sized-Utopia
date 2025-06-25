using System;
using System.Collections.Generic;
using UnityEngine;
public class Farm : ProductionBuilding
{
    public override void OnUpgraded() { }
    public override void OnDestroyed() { }
    
    public override void InitialSelfStorage()
    {
        AcceptResources = new HashSet<ResourceConfig>()
        {
            ResourceManager.Instance.GetConfig(ResourceType.Seed, (int)SeedSubType.Wheat),
            ResourceManager.Instance.GetConfig(ResourceType.Seed, (int)SeedSubType.Corn),
        };
        inventory = new Inventory(
            new List<ResourceStack>
            {
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Seed, (int)SeedSubType.Wheat), 10),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Seed, (int)SeedSubType.Corn), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Wheat), 0),
                ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Corn), 0),
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
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Seed, (int)SeedSubType.Wheat), 1),
                },
                outputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Wheat), 2)
                }
            },
            new ConversionRule()
            {
                inputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Seed, (int)SeedSubType.Corn), 1),
                },
                outputs = new List<ResourceStack>
                {
                    ResourceStack.CreateFromData(ResourceManager.Instance.GetConfig(ResourceType.Crop, (int)CropSubType.Corn), 2)
                }
            },
        };
    }
}