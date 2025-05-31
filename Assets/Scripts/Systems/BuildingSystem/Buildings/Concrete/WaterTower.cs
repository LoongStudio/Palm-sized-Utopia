using UnityEngine;

public class WaterTower : BuffBuilding
{
    public new void OnTryBuilt()
    {
        buffRadius = 4f;
        buffValue = 0.15f;
        targetBuildingType = BuildingType.Production;
        Debug.Log($"水塔建造完成，位置: {string.Join(" ", positions)}，影响范围: {buffRadius}");
    }
    
    public override void OnUpgraded()
    {
        buffRadius += 0.5f; // 升级增加影响范围
        Debug.Log($"水塔升级到等级 {currentLevel}，影响范围: {buffRadius}");
    }
    
    public override void OnDestroyed()
    {
        base.OnDestroyed();
        Debug.Log($"水塔被摧毁，位置: {string.Join(" ", positions)}");
    }

    public override float GetCurrentEfficiency()
    {
        return buffValue;
    }
} 