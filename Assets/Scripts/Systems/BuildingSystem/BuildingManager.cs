using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingManager : SingletonManager<BuildingManager>
{
    private HashSet<Building> _buildings;
    private Dictionary<BuildingSubType, BuildingData> _buildingDataDict;
    private Dictionary<Vector2Int, Building> _buildingOccupies;
    // 事件
    public static event System.Action<Building> OnBuildingBuilt;
    public static event System.Action<Building> OnBuildingUpgraded;
    public static event System.Action<Building> OnBuildingDestroyed;
    protected override void Awake()
    {
        base.Awake();
    }
    public void Initialize()
    {
        // TODO: 后续改为从存档中读取
        _buildings = new HashSet<Building>();
        _buildingDataDict = new Dictionary<BuildingSubType, BuildingData>();
        _buildingOccupies = new Dictionary<Vector2Int, Building>();
    }
    
    // 建筑管理
    /// <summary>
    /// 用于给Building Base 类OnBuild 事件调用用于注册
    /// </summary>
    /// <param name="building"></param>
    public bool BuildingBuilt(Building building)
    {
        // 注册建筑占用
        foreach (var buildingPosition in building.positions)
            if (_buildingOccupies.ContainsKey(buildingPosition)) return false;
        foreach (var buildingPosition in building.positions)
            _buildingOccupies[buildingPosition] = building;
        // 呼叫事件
        OnBuildingBuilt?.Invoke(building);
        return true;
    }
    public bool BuildBuilding(BuildingSubType type, Vector2Int position) { return false; }
    public bool UpgradeBuilding(Building building) { return false; }
    public bool DestroyBuilding(Building building) { return false; }
    public bool RegistBuilding(Building building)
    {
        if (_buildings.Contains(building)) return false;
        _buildings.Add(building);
        return true;
    }
    // 查询方法
    public List<Building> GetAllBuildings() { return _buildings.ToList(); }
    public List<Building> GetBuildingsByType(BuildingType type)
    {
        List<Building> buildingsReturn = new List<Building>();
        foreach (Building building in _buildings)
            if (building.data.buildingType == type)
                buildingsReturn.Add(building);
        return buildingsReturn;
    }
    public Building GetBuildingAt(Vector2Int position) { return _buildingOccupies[position]; }
    
    // 建筑解锁检查
    public bool IsBuildingUnlocked(BuildingSubType type) { return false; }
}