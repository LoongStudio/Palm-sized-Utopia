using System.Collections.Generic;
using UnityEngine;

public class Ranch : ProductionBuilding
{
    [Header("牧场专属")]
    public LivestockSubType animalType = LivestockSubType.Cattle;
    
    public new void OnTryBuilt()
    {
        status = BuildingStatus.Active;
        SetupProductionRule();
        Debug.Log($"牧场建造完成，位置: {string.Join(" ", positions)}");
    }
    
    public override void OnUpgraded()
    {
        Debug.Log($"牧场升级到等级 {currentLevel}");
    }
    
    public override void OnDestroyed()
    {
        StopProduction();
        Debug.Log($"牧场被摧毁，位置: {string.Join(" ", positions)}");
    }
    
    protected override void SetupProductionRule()
    {
        productionRules = new List<ConversionRule>()
        {
            new ConversionRule()
            {
                inputs = new List<ConversionResource> 
                { 
                    new ConversionResource(ResourceType.Feed, FeedSubType.Feed, 2),
                    new ConversionResource(
                        ResourceType.BreedingStock, 
                        BreedingStockSubType.Cattle, 1)
                },
                outputs = new List<ConversionResource> 
                { 
                    new ConversionResource(ResourceType.Livestock, LivestockSubType.Cattle, 1)
                },
                conversionTime = 20f
            },
            new ConversionRule()
            {
                inputs = new List<ConversionResource> 
                { 
                    new ConversionResource(ResourceType.Feed, FeedSubType.Feed, 2),
                    new ConversionResource(
                        ResourceType.BreedingStock, 
                        BreedingStockSubType.Sheep, 1)
                },
                outputs = new List<ConversionResource> 
                { 
                    new ConversionResource(ResourceType.Livestock, LivestockSubType.Sheep, 1)
                },
                conversionTime = 20f
            }
        };
    }
    
    public void ChangeAnimalType(LivestockSubType newAnimalType)
    {
        if (productionTimer <= 0)
        {
            animalType = newAnimalType;
            SetupProductionRule();
        }
    }
} 