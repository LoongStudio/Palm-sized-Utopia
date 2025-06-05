using System.Collections.Generic;
using UnityEngine;

public abstract class ProductionBuilding : Building, IResourceProducer
{
    [Header("生产属性")]
    public List<ConversionRule> productionRules;
    public float productionTimer;
    public float productionCooldown = 5f;

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
    protected virtual void UpdateProduction() { }
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