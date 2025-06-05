using System.Collections.Generic;
using UnityEngine;

public class WaterTower : BuffBuilding
{
    protected override void InitBuildingAndBuffTypes()
    {
        targetBuildingSubTypes = new List<BuildingSubType>()
        {
            BuildingSubType.Farm
        };
        targetBuffTypes = new List<BuffEnums>()
        {
            BuffEnums.WellIrrigated
        };
    }
    public override void OnUpgraded()
    {

    }
    
    public override void OnDestroyed()
    {
        base.OnDestroyed();
        Debug.Log($"水塔被摧毁，位置: {string.Join(" ", positions)}");
    }
    
} 