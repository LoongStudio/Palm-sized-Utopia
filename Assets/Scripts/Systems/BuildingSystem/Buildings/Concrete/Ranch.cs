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
    protected override void UpdateProduction()
    {
        base.UpdateProduction();
    }
    
    public void ChangeAnimalType(LivestockSubType newAnimalType)
    {
    }
} 