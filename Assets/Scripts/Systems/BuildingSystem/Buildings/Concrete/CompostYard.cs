using System;
using System.Collections.Generic;
using UnityEngine;

public class CompostYard : ProductionBuilding
{
    public override void OnUpgraded()
    {
        Debug.Log($"堆肥场升级到等级 {currentLevel}");
    }
    
    public override void OnDestroyed()
    {
        StopProduction();
        Debug.Log($"堆肥场被摧毁，位置: {string.Join(" ", positions)}");
    }
    
    public override void InitialSelfStorage()
    {
        AcceptResources = new List<Enum>() { CropSubType.Wheat, CropSubType.Corn };
        inventory = new Inventory(
            new List<SubResourceValue<int>>()
            {
                new SubResourceValue<int>(CropSubType.Wheat, 0),
                new SubResourceValue<int>(CropSubType.Corn, 0),
                new SubResourceValue<int>(FeedSubType.Feed, 0),
            },
            new List<SubResourceValue<int>>()
            {
                new SubResourceValue<int>(CropSubType.Wheat, 10),
                new SubResourceValue<int>(CropSubType.Corn, 10),
                new SubResourceValue<int>(FeedSubType.Feed, 25),
            });
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