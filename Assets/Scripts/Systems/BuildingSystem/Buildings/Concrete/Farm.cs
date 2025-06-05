using System;
using System.Collections.Generic;
using UnityEngine;
public class Farm : ProductionBuilding
{
    [Header("农田专属")]
    public CropSubType cropType;
    
    public new void OnTryBuilt() { }
    public override void OnUpgraded() { }
    public override void OnDestroyed() { }
    
    public override bool CanProduce()
    {
        // 检查是否有种子资源
        return false;
    }
    
    public override void ProduceResources()
    {
        // 消耗种子，生产作物和饲料
    }
    
    public override void InitialSelfStorage()
    {
        AcceptResources = new List<Enum>() { SeedSubType.Wheat, SeedSubType.Corn };
        currentSubResource = new List<SubResourceValue<int>>
        {
            new SubResourceValue<int>(SeedSubType.Wheat, 0),
            new SubResourceValue<int>(SeedSubType.Corn, 0),
            new SubResourceValue<int>(CropSubType.Wheat, 0),
            new SubResourceValue<int>(CropSubType.Corn, 0),
        };
        maximumSubResource = new List<SubResourceValue<int>>()
        {
            new SubResourceValue<int>(SeedSubType.Wheat, 30),
            new SubResourceValue<int>(SeedSubType.Corn, 30),
            new SubResourceValue<int>(CropSubType.Wheat, 30),
            new SubResourceValue<int>(CropSubType.Corn, 30),
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