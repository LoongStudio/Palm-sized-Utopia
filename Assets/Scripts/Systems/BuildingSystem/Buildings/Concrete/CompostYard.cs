using System.Collections.Generic;
using UnityEngine;

public class CompostYard : ProductionBuilding
{
    public override void OnBuilt()
    {
        status = BuildingStatus.Active;
        SetupProductionRule();
        Debug.Log($"堆肥场建造完成，位置: {position}");
    }
    
    public override void OnUpgraded()
    {
        Debug.Log($"堆肥场升级到等级 {currentLevel}");
    }
    
    public override void OnDestroyed()
    {
        StopProduction();
        Debug.Log($"堆肥场被摧毁，位置: {position}");
    }
    
    private void SetupProductionRule()
    {
        productionRule = new ConversionRule
        {
            inputs = new List<Resource> 
            { 
                new Resource(ResourceType.Crop, 0, 2) // 使用作物制作堆肥
            },
            outputs = new List<Resource> 
            { 
                new Resource(ResourceType.Feed, 0, 3) // 产出高质量饲料
            },
            conversionTime = 12f
        };
        productionCooldown = productionRule.conversionTime;
    }
} 