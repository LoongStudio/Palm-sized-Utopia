using UnityEngine;

public abstract class ProductionBuilding : Building, IResourceProducer
{
    [Header("生产属性")]
    public ConversionRule productionRule;
    public float productionTimer;
    public float productionCooldown;
    
    protected virtual void Update()
    {
        UpdateProduction();
    }
    
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