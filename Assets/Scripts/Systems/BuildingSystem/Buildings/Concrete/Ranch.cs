using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class Ranch : ProductionBuilding
{
    [SerializeField,ReadOnly,LabelText("当前优先生产的动物类型")] private LivestockSubType currentAnimalType;
    public override void OnUpgraded()
    {
        Debug.Log($"牧场升级到等级 {currentLevel}");
    }
    
    public override void OnDestroyed()
    {
        base.OnDestroyed();
        Debug.Log($"牧场被摧毁，位置: {string.Join(" ", positions)}");
    }

    public override void InitialSelfStorage()
    {

    }
    protected override void SetupProductionRule()
    {
        base.SetupProductionRule();

    }
    
    public void ChangeAnimalType(LivestockSubType newAnimalType)
    {
        if(newAnimalType == currentAnimalType) return;
        currentAnimalType = newAnimalType;
    }

    public override void ProduceResources()
    {
        // // 优先生产当前允许生产的动物，从生产规则中找到第一个产出物包括currentAnimalType的rule，然后生产
        // foreach (var rule in productionRules)
        // {
        //     List<ResourceStack> outputs = rule.outputs;
        //     if (outputs.Any(output => output.resourceConfig.type == ResourceType.Livestock 
        //     && output.resourceConfig.subType == (int)currentAnimalType))
        //     {
        //         bool exchanged = inventory.InternalProductionExchange(rule.inputs, rule.outputs);
        //         if (exchanged)
        //         {
        //             Debug.Log($"牧场生产了一次{currentAnimalType}");
        //             productionTimer = 0f; // 重置全局cd
        //             return;
        //         }
        //     }
        // }
        // 如果找不到，则进行正常生产
        base.ProduceResources();
    }
} 