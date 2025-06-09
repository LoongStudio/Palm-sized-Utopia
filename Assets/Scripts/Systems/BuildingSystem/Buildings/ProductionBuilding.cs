using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public abstract class ProductionBuilding : Building, IResourceProducer
{
    [Header("生产属性")]
    public List<ConversionRule> productionRules;
    public List<float> productionTimers;
    public float productionCooldown = 5f;
    public float conversionTime = 5f;
    public float baseEfficiency = 50f;
    public float efficiency = 1f;
    private bool _canProduce = true; 
    private new void Start()
    {
        base.Start();
        SetupProductionRule();
    }
    protected virtual void Update()
    {
        UpdateProduction();
    }
    protected virtual void SetupProductionRule() { }
    protected virtual void UpdateProduction()
    {
        if (!_canProduce) return;
        // 初始化计时器
        if (productionTimers.Count != productionRules.Count)
        {
            productionTimers = new List<float>(new float[productionRules.Count]);
        }

        ProduceResources();
    }

    public virtual void StartProduction() { _canProduce = true; }
    public virtual void StopProduction() { _canProduce = false; }
    public virtual bool CanProduce() { return _canProduce; }
    public virtual void ProduceResources()
    {
        UpdateCurrentEfficiency();

        for (int i = 0; i < productionRules.Count; i++)
        {
            var rule = productionRules[i];
            bool canProduce = true;

            // 检查输入资源是否足够
            foreach (var input in rule.inputs)
            {
                if (!inventory.HasEnough(input))
                {
                    canProduce = false;
                    break;
                }
            }

            // 检查输出资源是否有足够空间
            if (canProduce)
            {
                foreach (var output in rule.outputs)
                {
                    if (!inventory.CanReceive(output))
                    {
                        canProduce = false;
                        break;
                    }
                }
            }

            if (canProduce)
            {
                productionTimers[i] += Time.deltaTime;

                if (productionTimers[i] >= conversionTime / efficiency)
                {
                    // 消耗输入资源
                    foreach (var input in rule.inputs)
                    {
                        inventory.RemoveItem(input.subResource.resourceType, input.resourceValue);
                    }

                    // 增加输出资源
                    foreach (var output in rule.outputs)
                    {
                        inventory.AddItem(output.subResource.resourceType, output.resourceValue);
                    }

                    productionTimers[i] = 0f;
                    break; // 每周期最多执行一个
                }
            }
            else
            {
                productionTimers[i] = 0f;
            }
        }
    }

    
    public virtual float UpdateCurrentEfficiency()
    {
        float levelBonus = 0.1f * currentLevel;
        float npcEfficiencyPerNPC = 1f / maxSlotAmount; 
        float npcBonus = assignedNPCs != null ? assignedNPCs.Count * npcEfficiencyPerNPC : 0f;
        float deviceBonus = 0;
        if (installedEquipment != null)
            foreach (var equipment in installedEquipment)
                deviceBonus += equipment.deviceBonus;
        float totalEfficiency = baseEfficiency + levelBonus + npcBonus + deviceBonus;
        // float totalEfficiency = baseEfficiency;
        // 按产出规则数量分摊效率（防止多个规则时过快）
        if (productionRules != null && productionRules.Count > 0)
            totalEfficiency /= productionRules.Count;

        efficiency = totalEfficiency;
        return efficiency;
    }
}