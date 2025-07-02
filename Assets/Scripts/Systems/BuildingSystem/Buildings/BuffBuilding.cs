using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;


public abstract class BuffBuilding : Building
{
    [Header("加成属性")]
    public List<BuildingSubType> affectedBuildingSubTypes;
    public List<BuffEnums> affectedBuffTypes;
    
    public static event Action<BuffBuilding> OnBuffBuildingBuilt;
    public static event Action<BuffBuilding> OnBuffBuildingDestroyed;
    public override bool OnTryBuilt()
    {
        if (base.OnTryBuilt())
        {
            InitBuildingAndBuffTypes();
            return true;
        }
            
        return false;
    }
    
    protected abstract void InitBuildingAndBuffTypes();

    public override void Start()
    {
        // 调用基类的Start方法，确保正确的初始化流程
        base.Start();
        
        // 只有在建筑状态为活跃时才触发加成建筑建造事件
        if (status == BuildingStatus.Active)
        {
            OnBuffBuildingBuilt?.Invoke(this);
        }
    }
    
    public override void OnDestroyed()
    {
        OnBuffBuildingDestroyed?.Invoke(this);
        base.OnDestroyed();
    }
    
    // protected virtual void FindAffectedBuildings() { } 现在直接全局搜索apply
}