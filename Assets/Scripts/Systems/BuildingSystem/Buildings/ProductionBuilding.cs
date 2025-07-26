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
    public float productionTimer = 0f; // 全局cd
    public float conversionTime = 5f;
    public float productionSpeedMultiplier = 1f;
    private bool _canProduce = true; 
    public bool randomProductionOrder = false; // 新增：是否随机生产顺序
    private void OnEnable(){
        GameEvents.OnNPCInWorkingPosition += UpdateCurrentEfficiencyOnEvent;
        GameEvents.OnNPCLeaveWorkingPosition += UpdateCurrentEfficiencyOnEvent;
    }
    private void OnDisable(){
        GameEvents.OnNPCInWorkingPosition -= UpdateCurrentEfficiencyOnEvent;
        GameEvents.OnNPCLeaveWorkingPosition -= UpdateCurrentEfficiencyOnEvent;
    }
    private new void Start()
    {
        base.Start();
        SetupProductionRule();
    }
    protected virtual void Update()
    {
        UpdateProduction();
    }
    // TODO: 重写Building的SetBuildingData方法，设置生产数据, 不用SetupProductionRule
    protected virtual void SetupProductionRule() { }

    public override void SetBuildingData(BuildingPrefabData data)
    {
        base.SetBuildingData(data);
        // 设置转换规则
        productionRules = data.productionBuildingDatas.conversionRules;
        productionCooldown = data.productionBuildingDatas.productionCooldown;
    }

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
    
    /// <summary>
    /// 交换两个生产规则的位置
    /// </summary>
    public void SwitchProductionRuleOrder(int i, int j)
    {
        if (productionRules == null || i < 0 || j < 0 || i >= productionRules.Count || j >= productionRules.Count || i == j)
            return;
        (productionRules[i], productionRules[j]) = (productionRules[i], productionRules[j]);
    }

    public virtual void ProduceResources()
    {

        // 冷却未结束，直接返回
        if (productionTimer < productionCooldown / productionSpeedMultiplier)
        {
            productionTimer += Time.deltaTime;
            return;
        }
        
        // 重置计时器, 防止短时间内多次运行复杂逻辑
        productionTimer = 0f;
        UpdateCurrentEfficiency();

        // 新增：根据开关决定生产顺序
        List<int> indices = new List<int>();
        for (int i = 0; i < productionRules.Count; i++) indices.Add(i);
        if (randomProductionOrder)
        {
            // 洗牌算法
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int swap = UnityEngine.Random.Range(0, i + 1);
                (indices[i], indices[swap]) = (indices[swap], indices[i]);
            }
        }

        // 遍历所有rule，找到第一个能生产的
        foreach (int idx in indices)
        {
            var rule = productionRules[idx];
            bool exchanged = inventory.InternalProductionExchange(rule.inputs, rule.outputs);
            if (exchanged)
            {
                productionTimer = 0f; // 重置全局cd
                break;
            }
        }
    }

    private void UpdateCurrentEfficiencyOnEvent(NPCEventArgs args){
        if(args != null){
            if(args.relatedBuilding != this){
                return;
            }
        }
        UpdateCurrentEfficiency();
    }
    public virtual float UpdateCurrentEfficiency()
    {
        float levelBonus = 0.1f * currentLevel;
        float slotBonus = GetNPCSlotBuff().intensity/100f;
        float npcAbilityBonus = GetInSlotNPCBuff().intensity/100f;
        float friendWorkTogetherBonus = GetFriendWorkTogetherBuff().intensity/100f;
        float deviceBonus = 0;
        if (installedEquipment != null)
            foreach (var equipment in installedEquipment)
                deviceBonus += equipment.deviceBonus;
        float totalEfficiency = BaseProductionSpeedMultiplier + levelBonus + slotBonus + npcAbilityBonus + friendWorkTogetherBonus + deviceBonus;
        Debug.Log($"[ProductionBuilding] 建筑{data.buildingName}当前总效率{totalEfficiency}|基础效率{BaseProductionSpeedMultiplier}|等级加成{levelBonus}|槽位加成{slotBonus}|NPC能力加成{npcAbilityBonus}|友方合作加成{friendWorkTogetherBonus}|设备加成{deviceBonus}");
        // float totalEfficiency = baseEfficiency;
        // 按产出规则数量分摊效率（防止多个规则时过快）
        // if (productionRules != null && productionRules.Count > 0)
        //     totalEfficiency /= productionRules.Count;

        productionSpeedMultiplier = totalEfficiency;
        return productionSpeedMultiplier;
    }

    /// <summary>
    /// 检查当前库存是否能完整执行指定生产规则（不会因产出溢出导致回滚，且能消耗输入）
    /// </summary>
    public bool CanProduceByRule(int ruleIndex)
    {
        if (productionRules == null || ruleIndex < 0 || ruleIndex >= productionRules.Count)
            return false;
        var rule = productionRules[ruleIndex];
        // 检查输入资源是否足够
        foreach (var input in rule.inputs)
        {
            var stack = inventory.currentStacks.Find(s => s.resourceConfig.Equals(input.resourceConfig));
            if (stack == null || stack.amount < input.amount)
                return false;
        }
        // 检查输出资源是否有足够空间
        foreach (var output in rule.outputs)
        {
            var stack = inventory.currentStacks.Find(s => s.resourceConfig.Equals(output.resourceConfig));
            int current = stack != null ? stack.amount : 0;
            int limit = stack != null ? stack.storageLimit : inventory.defaultMaxValue;
            // storageLimit==0 视为无限
            if (limit > 0 && current + output.amount > limit)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 检查当前库存是否能执行任意一个生产规则
    /// </summary>
    public bool CanProduceAnyRule()
    {
        if (productionRules == null || productionRules.Count == 0)
            return false;
        for (int i = 0; i < productionRules.Count; i++)
        {
            if (CanProduceByRule(i))
                return true;
        }
        return false;
    }
}