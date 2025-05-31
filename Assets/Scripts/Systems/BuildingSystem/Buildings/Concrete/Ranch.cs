using System.Collections.Generic;
using UnityEngine;

public class Ranch : ProductionBuilding
{
    [Header("牧场专属")]
    public AnimalSubType animalType = AnimalSubType.Cattle;
    
    public override void OnBuilt()
    {
        status = BuildingStatus.Active;
        SetupProductionRule();
        Debug.Log($"牧场建造完成，位置: {position}");
    }
    
    public override void OnUpgraded()
    {
        Debug.Log($"牧场升级到等级 {currentLevel}");
    }
    
    public override void OnDestroyed()
    {
        StopProduction();
        Debug.Log($"牧场被摧毁，位置: {position}");
    }
    
    private void SetupProductionRule()
    {
        productionRule = new ConversionRule
        {
            inputs = new List<Resource> 
            { 
                new Resource(ResourceType.Feed, 0, 2),
                new Resource(ResourceType.Livestock, (int)animalType, 1)
            },
            outputs = new List<Resource> 
            { 
                new Resource(ResourceType.Animal, (int)animalType, 1)
            },
            conversionTime = 20f
        };
        productionCooldown = productionRule.conversionTime;
    }
    
    public void ChangeAnimalType(AnimalSubType newAnimalType)
    {
        if (productionTimer <= 0)
        {
            animalType = newAnimalType;
            SetupProductionRule();
        }
    }
} 