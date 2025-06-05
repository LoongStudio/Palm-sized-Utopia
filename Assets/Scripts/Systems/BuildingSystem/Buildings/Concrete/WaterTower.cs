using System.Collections.Generic;
using UnityEngine;

public class WaterTower : BuffBuilding
{
    protected override void InitBuildingAndBuffTypes()
    {
        affectedBuildingSubTypes = new List<BuildingSubType>()
        {
            BuildingSubType.Farm
        };
        affectedBuffTypes = new List<BuffEnums>()
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