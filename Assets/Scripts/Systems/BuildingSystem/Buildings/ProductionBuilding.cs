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
        // 初始化计时器
        if (productionTimers.Count != productionRules.Count)
        {
            productionTimers = new List<float>(new float[productionRules.Count]);
        }

        for (int i = 0; i < productionRules.Count; i++)
        {
            var rule = productionRules[i];
            bool canProduce = true;

            // 检查输入资源是否足够
            foreach (var input in rule.inputs)
            {
                var current = currentSubResource.FirstOrDefault(r =>
                    r.resourceType == input.resourceType &&
                    r.subType == input.subType);

                if (current == null || current.resourceValue < input.resourceValue)
                {
                    canProduce = false;
                    break;
                }
            }

            // 检查输出资源是否会超出上限
            if (canProduce)
            {
                foreach (var output in rule.outputs)
                {
                    var current = currentSubResource.FirstOrDefault(r =>
                        r.resourceType == output.resourceType &&
                        r.subType == output.subType);

                    var max = maximumSubResource.FirstOrDefault(r =>
                        r.resourceType == output.resourceType &&
                        r.subType == output.subType);

                    int currentValue = current?.resourceValue ?? 0;
                    int maxValue = max?.resourceValue ?? 0;

                    if (currentValue + output.resourceValue > maxValue)
                    {
                        canProduce = false;
                        break;
                    }
                }
            }

            // 开始生产逻辑
            if (canProduce)
            {
                productionTimers[i] += Time.deltaTime;

                if (productionTimers[i] >= conversionTime)
                {
                    // 消耗输入资源
                    foreach (var input in rule.inputs)
                    {
                        var item = currentSubResource.FirstOrDefault(r =>
                            r.resourceType == input.resourceType &&
                            r.subType == input.subType);

                        if (item != null)
                            item.resourceValue -= input.resourceValue;
                    }

                    // 增加输出资源
                    foreach (var output in rule.outputs)
                    {
                        var item = currentSubResource.FirstOrDefault(r =>
                            r.resourceType == output.resourceType &&
                            r.subType == output.subType);

                        if (item != null)
                            item.resourceValue += output.resourceValue;
                        else
                            currentSubResource.Add(new SubResourceValue<int>(
                                SubResourceValue<int>.GetEnumFromTypeAndIndex(output.resourceType, output.subType), 
                                output.resourceValue));
                    }

                    productionTimers[i] = 0f;
                    break; // TODO: 每一次生产周期只会进行一次转化
                }
            }
            else
            {
                // 不满足生产条件则暂停计时
                productionTimers[i] = 0f;
            }
        }
    }

    public virtual void StartProduction() { }
    public virtual void StopProduction() { }
    public virtual bool CanProduce() { return false; }
    public virtual void ProduceResources() { }
    
    public override float GetCurrentEfficiency()
    {
        // 计算综合效率：基础效率 + 等级加成 + NPC加成 + 设备加成 + 加成建筑影响
        return 0f;
    }
}