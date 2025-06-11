using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class BuildingManager : SingletonManager<BuildingManager>
{
    private List<Building> _buildings;
    private Dictionary<BuildingSubType, BuildingData> _buildingDataDict;
    private Dictionary<Vector2Int, Building> _buildingOccupies;
    // 历史资源变化记录（可用于曲线绘制）
    private Dictionary<(ResourceType, Enum), List<int>> resourceHistory = new();
    // 事件
    public static event System.Action<Building> OnBuildingBuilt;
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action<Building> OnBuildingUpgraded;
    // public static event System.Action<Building> OnBuildingDestroyed;
    // Buff 加成
    public Dictionary<BuildingSubType, Dictionary<BuffEnums, int>> AppliedBuffs;
    
    private void OnEnable()
    {
        BuffBuilding.OnBuffBuildingBuilt += HandleBuffBuildingBuilt;
        BuffBuilding.OnBuffBuildingDestroyed += HandleBuffBuildingDestroyed;
    }

    private void OnDisable()
    {
        BuffBuilding.OnBuffBuildingBuilt -= HandleBuffBuildingBuilt;
        BuffBuilding.OnBuffBuildingDestroyed -= HandleBuffBuildingDestroyed;
    }
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    protected void Start()
    {
        if (Time.frameCount % 5 == 0)
            RecordResourceSnapshot();
    }
    public void Initialize()
    {
        // TODO: 后续改为从存档中读取
        _buildings = new List<Building>();
        _buildingDataDict = new Dictionary<BuildingSubType, BuildingData>();
        _buildingOccupies = new Dictionary<Vector2Int, Building>();
        AppliedBuffs = new Dictionary<BuildingSubType, Dictionary<BuffEnums, int>>();
    }
    
    
    /// <summary>
    /// 记录当前帧资源分布
    /// </summary>
    private void RecordResourceSnapshot()
    {
        var total = GetTotalResource();

        foreach (var kvp in total)
        {
            if (!resourceHistory.ContainsKey(kvp.Key))
                resourceHistory[kvp.Key] = new List<int>();

            resourceHistory[kvp.Key].Add(kvp.Value);
        }
    }

    /// <summary>
    /// 汇总所有建筑的当前资源
    /// </summary>
    public Dictionary<(ResourceType, Enum), int> GetTotalResource()
    {
        Dictionary<(ResourceType, Enum), int> result = new();

        foreach (var building in _buildings)
        {
            foreach (var res in building.inventory.currentSubResource)
            {
                // 通过 ResourceType 拿到对应的 Enum 类型
                if (!SubResource.MappingMainSubType.TryGetValue(
                    res.subResource.resourceType, out var enumType))
                    continue;

                // 将 int subType 转换为 Enum 实例
                Enum subTypeEnum = (Enum)Enum.ToObject(enumType, res.subResource.subType);

                var key = (res.subResource.resourceType, subTypeEnum);
                if (!result.ContainsKey(key))
                    result[key] = 0;

                result[key] += res.resourceValue;
            }
        }


        return result;
    }

    /// <summary>
    /// 找出等待输入资源的建筑（AcceptResources中资源未达到最大值）
    /// </summary>
    public List<(Building, SubResource)> GetBuildingsWaitingForResources()
    {
        List<(Building, SubResource)> result = new();

        foreach (var building in _buildings)
        {
            foreach (var res in building.AcceptResources)
            {
                int targetSubType = Convert.ToInt32(res);

                var current = building.inventory.currentSubResource
                    .FirstOrDefault(r => r.subResource.subType == targetSubType)?.resourceValue ?? 0;

                var max = building.inventory.maximumSubResource
                    .FirstOrDefault(r => r.subResource.subType == targetSubType)?.resourceValue ?? 0;

                if (current < max)
                    result.Add((building, res));
            }
        }

        return result;
    }


    /// <summary>
    /// 找出拥有富余资源的建筑（资源未被限制且有剩余）
    /// </summary>
    public List<(Building, SubResourceValue<int>)> GetBuildingsWithExcessResources()
    {
        List<(Building, SubResourceValue<int>)> result = new();

        foreach (var building in _buildings)
        {
            foreach (var res in building.inventory.currentSubResource)
            {
                var maxEntry = building.inventory.maximumSubResource.FirstOrDefault(r =>
                    r.subResource.resourceType == res.subResource.resourceType &&
                    r.subResource.subType == res.subResource.subType);

                int max = maxEntry?.resourceValue ?? 0;

                // 富余条件：超过最大值的80%，或者最大值为0视为无上限
                if (max == 0 || res.resourceValue > 0.8f * max)
                {
                    result.Add((building, res));
                }
            }
        }

        return result;
    }


    /// <summary>
    /// 获取资源增长曲线（某个资源）
    /// </summary>
    public List<int> GetResourceHistory(ResourceType type, Enum subType)
    {
        var key = (type, subType);
        return resourceHistory.TryGetValue(key, out var list) ? list : new List<int>();
    }

    /// <summary>
    /// 获取指定资源的总量
    /// </summary>
    public int GetResourceAmount(ResourceType type, Enum subType)
    {
        int targetSubType = Convert.ToInt32(subType);

        return _buildings
            .SelectMany(b => b.inventory.currentSubResource)
            .Where(r =>
                r.subResource.resourceType == type &&
                r.subResource.subType == targetSubType)
            .Sum(r => r.resourceValue);
    }


    public void HandleBuffBuildingBuilt(BuffBuilding building)
    {
        foreach (var targetBuildingSubType in building.affectedBuildingSubTypes)
        {
            if (!AppliedBuffs.ContainsKey(targetBuildingSubType)) 
                AppliedBuffs.Add(targetBuildingSubType, new Dictionary<BuffEnums, int>());
            foreach (var targetBuffType in building.affectedBuffTypes)
            {
                if (!AppliedBuffs[targetBuildingSubType].ContainsKey(targetBuffType))
                    AppliedBuffs[targetBuildingSubType][targetBuffType] = 1;
                else
                    AppliedBuffs[targetBuildingSubType][targetBuffType] += 1;
            }
        }
        
    }

    public void HandleBuffBuildingDestroyed(BuffBuilding building)
    {
        foreach (var targetBuildingSubType in building.affectedBuildingSubTypes)
        {
            if (!AppliedBuffs.ContainsKey(targetBuildingSubType)) 
                Debug.LogError("Buff建筑移除错误，参数不匹配，不可能存在目标建筑参数不存在登记表的情况");
            
            foreach (var targetBuffType in building.affectedBuffTypes)
            {
                if (!AppliedBuffs[targetBuildingSubType].ContainsKey(targetBuffType))
                    Debug.LogError("Buff建筑移除错误，参数不匹配，不可能存在目标Buff参数不存在登记表的情况");
                else
                    AppliedBuffs[targetBuildingSubType][targetBuffType] -= 1;
                if (AppliedBuffs[targetBuildingSubType][targetBuffType] < 0)
                    Debug.LogError("Buff建筑移除错误，参数不匹配，不可能存在目标Buff参数数量小于0的情况");
            }
        }
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
    public bool RegisterBuilding(Building building)
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

    /// <summary>
    /// 获取需要工作的建筑列表，按优先级排序
    /// </summary>
    public List<Building> GetBuildingsNeedingWork()
    {
        List<Building> result = new List<Building>();
        
        foreach (var building in _buildings)
        {
            // 检查是否有空余槽位
            if (building.assignedNPCs.Count < building.maxSlotAmount)
            {
                result.Add(building);
                continue;
            }

            // 检查是否需要输入资源
            foreach (var res in building.AcceptResources)
            {
                int targetSubType = Convert.ToInt32(res);
                var current = building.inventory.currentSubResource
                    .FirstOrDefault(r => r.subResource.subType == targetSubType)?.resourceValue ?? 0;
                var max = building.inventory.maximumSubResource
                    .FirstOrDefault(r => r.subResource.subType == targetSubType)?.resourceValue ?? 0;

                if (current < max)
                {
                    result.Add(building);
                    break;
                }
            }

            // 检查是否需要转移资源
            foreach (var res in building.inventory.currentSubResource)
            {
                var maxEntry = building.inventory.maximumSubResource.FirstOrDefault(r =>
                    r.subResource.resourceType == res.subResource.resourceType &&
                    r.subResource.subType == res.subResource.subType);

                int max = maxEntry?.resourceValue ?? 0;

                // 如果资源超过最大值的80%，需要转移
                if (max > 0 && res.resourceValue > 0.8f * max)
                {
                    result.Add(building);
                    break;
                }
            }
        }

        // 按优先级排序：空余槽位 > 需要输入资源 > 需要转移资源
        return result.OrderBy(b => 
        {
            if (b.assignedNPCs.Count < b.maxSlotAmount) return 0;
            if (b.AcceptResources.Any()) return 1;
            return 2;
        }).ToList();
    }

    /// <summary>
    /// 获取最适合NPC工作的建筑
    /// </summary>
    public Building GetBestWorkBuildingForNPC(NPC npc)
    {
        var buildings = GetBuildingsNeedingWork();
        if (buildings.Count == 0) return null;

        // 优先选择有空余槽位的建筑
        var buildingWithSlot = buildings.FirstOrDefault(b => b.assignedNPCs.Count < b.maxSlotAmount);
        if (buildingWithSlot != null) return buildingWithSlot;

        // 其次选择需要输入资源的建筑
        var buildingNeedingInput = buildings.FirstOrDefault(b => b.AcceptResources.Any());
        if (buildingNeedingInput != null) return buildingNeedingInput;

        // 最后选择需要转移资源的建筑
        return buildings.FirstOrDefault();
    }
}