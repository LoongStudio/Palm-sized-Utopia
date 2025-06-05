using System.Collections.Generic;
using UnityEngine;

public class CompostYard : ProductionBuilding
{
    public new void OnTryBuilt()
    {
        status = BuildingStatus.Active;
        SetupProductionRule();
        Debug.Log($"堆肥场建造完成，位置: {string.Join(" ", positions)}");
    }
    
    public override void OnUpgraded()
    {
        Debug.Log($"堆肥场升级到等级 {currentLevel}");
    }
    
    public override void OnDestroyed()
    {
        StopProduction();
        Debug.Log($"堆肥场被摧毁，位置: {string.Join(" ", positions)}");
    }
    
    protected override void SetupProductionRule()
    {
        productionRules = new List<ConversionRule>()
        {
            new ConversionRule
            {
                inputs = new List<ConversionResource>
                {
                    new ConversionResource(ResourceType.Crop, CropSubType.Wheat, 2) // 使用作物制作堆肥
                },
                outputs = new List<ConversionResource>
                {
                    new ConversionResource(ResourceType.Feed, FeedSubType.Feed, 3) // 产出高质量饲料
                },
                conversionTime = 12f
            }
        };
    }
} 