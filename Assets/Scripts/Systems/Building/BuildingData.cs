using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("基本信息")]
    public BuildingType buildingType;
    public string buildingName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    
    [Header("建造属性")]
    public ResourceCost[] buildCosts;
    public int maxLevel = 1;
    public bool canBeUpgraded = false;
    
    [Header("功能属性")]
    public int npcCapacity = 0;         // NPC容量
    public int equipmentSlots = 0;      // 设备槽位
    public Vector2Int size = Vector2Int.one; // 占地大小
    
    [Header("生产属性")]
    public bool isProductionBuilding = false;
    public ResourceType[] producedResources;
    public float baseProductionRate = 1f;   // 基础生产率
    public int storageCapacity = 100;       // 存储容量
    
    [Header("升级配置")]
    public BuildingUpgradeData[] upgradeLevels;
    
    [Header("装饰属性")]
    public bool isDecorative = false;
    public int beautyValue = 0;             // 美观度
    public float npcMoodBonus = 0f;         // NPC心情加成
    
    public ResourceCost[] GetUpgradeCost(int currentLevel)
    {
        if (currentLevel < upgradeLevels.Length)
        {
            return upgradeLevels[currentLevel].upgradeCosts;
        }
        return new ResourceCost[0];
    }
    
    public BuildingUpgradeData GetUpgradeData(int level)
    {
        if (level > 0 && level <= upgradeLevels.Length)
        {
            return upgradeLevels[level - 1];
        }
        return null;
    }
}

[System.Serializable]
public class BuildingUpgradeData
{
    public int level;
    public ResourceCost[] upgradeCosts;
    public float productionRateMultiplier = 1f;
    public int storageCapacityBonus = 0;
    public int npcCapacityBonus = 0;
    public int equipmentSlotsBonus = 0;
    public string upgradeDescription;
}