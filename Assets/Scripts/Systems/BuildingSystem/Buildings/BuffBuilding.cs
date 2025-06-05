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

    public virtual void Start()
    {
        OnTryBuilt();
        OnBuffBuildingBuilt?.Invoke(this);
    }
    
    public override void OnDestroyed()
    {
        OnBuffBuildingDestroyed?.Invoke(this);
    }
    
    // protected virtual void FindAffectedBuildings() { } 现在直接全局搜索apply
}