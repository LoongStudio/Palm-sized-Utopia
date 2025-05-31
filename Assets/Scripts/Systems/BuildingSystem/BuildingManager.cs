using UnityEngine;
using System.Collections.Generic;
public class BuildingManager : SingletonManager<BuildingManager>
{
    private List<Building> buildings;
    private Dictionary<BuildingSubType, BuildingData> buildingDataDict;
    
    // 事件
    public static event System.Action<Building> OnBuildingBuilt;
    public static event System.Action<Building> OnBuildingUpgraded;
    public static event System.Action<Building> OnBuildingDestroyed;
    
    public void Initialize() { }
    
    // 建筑管理
    public bool BuildBuilding(BuildingSubType type, Vector2Int position) { return false; }
    public bool UpgradeBuilding(Building building) { return false; }
    public bool DestroyBuilding(Building building) { return false; }
    
    // 查询方法
    public List<Building> GetAllBuildings() { return null; }
    public List<Building> GetBuildingsByType(BuildingType type) { return null; }
    public Building GetBuildingAt(Vector2Int position) { return null; }
    
    // 建筑解锁检查
    public bool IsBuildingUnlocked(BuildingSubType type) { return false; }
}