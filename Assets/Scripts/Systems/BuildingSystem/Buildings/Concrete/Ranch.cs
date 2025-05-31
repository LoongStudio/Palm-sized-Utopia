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
    
    private void SetupProductionRule()
    {
        productionRule = new ConversionRule
        {
            inputs = new List<Resource> 
            { 
                new Resource(ResourceType.Feed, 0, 2),
                new Resource(ResourceType.BreedingStock, (int)animalType, 1)
            },
            outputs = new List<Resource> 
            { 
                new Resource(ResourceType.Livestock, (int)animalType, 1)
            },
            conversionTime = 20f
        };
        productionCooldown = productionRule.conversionTime;
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