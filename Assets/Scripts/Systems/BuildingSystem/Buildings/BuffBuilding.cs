using UnityEngine;
using System.Collections.Generic;

public abstract class BuffBuilding : Building
{
    [Header("加成属性")]
    public float buffRadius;
    public float buffValue;
    public BuildingType targetBuildingType;
    
    protected List<Building> affectedBuildings;
    
    public override void OnBuilt()
    {
        FindAffectedBuildings();
        ApplyBuffs();
    }
    
    public override void OnDestroyed()
    {
        RemoveBuffs();
    }
    
    protected virtual void FindAffectedBuildings() { }
    protected virtual void ApplyBuffs() { }
    protected virtual void RemoveBuffs() { }
    public virtual float GetBuffValue() { return 0f; }
}