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
        StopProduction();
        Debug.Log($"牧场被摧毁，位置: {string.Join(" ", positions)}");
    }

    public override void InitialSelfStorage()
    {
        AcceptResources = new List<Enum>() { FeedSubType.Feed, BreedingStockSubType.Cattle, BreedingStockSubType.Sheep };
        currentSubResource = new List<SubResourceValue<int>>()
        {
            new SubResourceValue<int>(FeedSubType.Feed, 0),
            new SubResourceValue<int>(BreedingStockSubType.Cattle, 0),
            new SubResourceValue<int>(BreedingStockSubType.Sheep, 0),
            new SubResourceValue<int>(LivestockSubType.Cattle, 0),
            new SubResourceValue<int>(LivestockSubType.Sheep, 0),
        };
        maximumSubResource = new List<SubResourceValue<int>>()
        {
            new SubResourceValue<int>(FeedSubType.Feed, 50),
            new SubResourceValue<int>(BreedingStockSubType.Cattle, 10),
            new SubResourceValue<int>(BreedingStockSubType.Sheep, 10),
            new SubResourceValue<int>(LivestockSubType.Cattle, 10),
            new SubResourceValue<int>(LivestockSubType.Sheep, 10),
        };
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
                    new SubResourceValue<int>(FeedSubType.Feed, 2),
                    new SubResourceValue<int>(BreedingStockSubType.Cattle, 1)
                },
                outputs = new List<SubResourceValue<int>> 
                { 
                    new SubResourceValue<int>(LivestockSubType.Cattle, 1)
                }
            },
            new ConversionRule()
            {
                inputs = new List<SubResourceValue<int>> 
                { 
                    new SubResourceValue<int>(FeedSubType.Feed, 2),
                    new SubResourceValue<int>(BreedingStockSubType.Sheep, 1)
                },
                outputs = new List<SubResourceValue<int>> 
                { 
                    new SubResourceValue<int>(LivestockSubType.Sheep, 1)
                }
            }
        };
    }
    
    public void ChangeAnimalType(LivestockSubType newAnimalType)
    {
    }
} 