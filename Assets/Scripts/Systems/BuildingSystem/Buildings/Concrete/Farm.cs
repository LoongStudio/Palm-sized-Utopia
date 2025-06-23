using System;
using System.Collections.Generic;
using UnityEngine;
public class Farm : ProductionBuilding
{
    public override void OnUpgraded() { }
    public override void OnDestroyed() { }
    
    public override void InitialSelfStorage()
    {
        AcceptResources = new List<SubResource>()
        {
            SubResource.CreateFromEnum(SeedSubType.Wheat), 
            SubResource.CreateFromEnum(SeedSubType.Corn)
        };
        inventory = new Inventory(
            new List<SubResourceValue<int>>
            {
                new SubResourceValue<int>(SeedSubType.Wheat, 10),
                new SubResourceValue<int>(SeedSubType.Corn, 0),
                new SubResourceValue<int>(CropSubType.Wheat, 0),
                new SubResourceValue<int>(CropSubType.Corn, 0),
            },
            new List<SubResourceValue<int>>()
            {
                new SubResourceValue<int>(SeedSubType.Wheat, 30),
                new SubResourceValue<int>(SeedSubType.Corn, 30),
                new SubResourceValue<int>(CropSubType.Wheat, 30),
                new SubResourceValue<int>(CropSubType.Corn, 30),
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
                inputs = new List<SubResourceValue<int>> 
                { 
                    new SubResourceValue<int>(SeedSubType.Wheat, 1),
                },
                outputs = new List<SubResourceValue<int>> 
                { 
                    new SubResourceValue<int>(CropSubType.Wheat, 2)
                }
            },
            new ConversionRule()
            {
                inputs = new List<SubResourceValue<int>> 
                { 
                    new SubResourceValue<int>(SeedSubType.Corn, 1),
                },
                outputs = new List<SubResourceValue<int>> 
                { 
                    new SubResourceValue<int>(CropSubType.Corn, 2)
                }
            },
        };
    }
}