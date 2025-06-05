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
        base.SetupProductionRule();
        productionRules = new List<ConversionRule>()
        {
            new ConversionRule
            {
                inputs = new List<SubResourceValue<int>>
                {
                    new SubResourceValue<int>(CropSubType.Wheat, 2) // 使用作物制作堆肥
                },
                outputs = new List<SubResourceValue<int>>
                {
                    new SubResourceValue<int>(FeedSubType.Feed, 3) // 产出高质量饲料
                }
            }
        };
    }
} 