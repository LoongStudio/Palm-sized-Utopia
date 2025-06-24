using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class BuildingManager : SingletonManager<BuildingManager>
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    private List<Building> _buildings;
    private Dictionary<BuildingSubType, BuildingData> _buildingDataDict;
    private Dictionary<Vector2Int, Building> _buildingOccupies;
    // 历史资源变化记录（可用于曲线绘制）
    private Dictionary<DateTime, List<ResourceStack>> resourceHistory = new();
    // 事件
    public static event System.Action<Building> OnBuildingBuilt;
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action<Building> OnBuildingUpgraded;
    // public static event System.Action<Building> OnBuildingDestroyed;
    // Buff 加成
    public Dictionary<BuildingSubType, Dictionary<BuffEnums, int>> AppliedBuffs;
    
    // 资源需求与输出表
    private Dictionary<(ResourceType, int), List<Building>> resourceNeeds = new();
    private Dictionary<(ResourceType, int), List<Building>> resourceOutputs = new();

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
        resourceHistory[DateTime.Now] = GetTotalResource();
    }

    /// <summary>
    /// 汇总所有建筑的当前资源
    /// </summary>
    public List<ResourceStack> GetTotalResource()
    {
        Dictionary<ResourceConfig, ResourceStack> resourceStacks = new();
        foreach (var building in _buildings)
        {
            foreach (var stack in building.inventory.currentStacks)
            {
                if (resourceStacks.ContainsKey(stack.resourceConfig)) 
                    resourceStacks[stack.resourceConfig].AddAmount(stack.amount);
                else
                    resourceStacks[stack.resourceConfig] = stack.Clone();
            }
        }
        List<ResourceStack> result = resourceStacks.Values.ToList();
        return result;
    }

    /// <summary>
    /// 找出等待输入资源的建筑（AcceptResources中资源未达到最大值）
    /// </summary>
    public List<(Building, ResourceConfig)> GetBuildingsWaitingForResources()
    {
        List<(Building, ResourceConfig)> result = new();

        foreach (var building in _buildings)
        {
            foreach (var res in building.AcceptResources)
            {
                var stack = building.inventory.currentStacks
                    .FirstOrDefault(r => r.resourceConfig.Equals(res));

                if (stack == null || stack.amount < stack.storageLimit)
                    result.Add((building, res));
            }
        }
        return result;
    }


    /// <summary>
    /// 找出拥有富余资源的建筑（资源未被限制且有剩余）
    /// </summary>
    public List<(Building, ResourceStack)> GetBuildingsWithExcessResources()
    {
        List<(Building, ResourceStack)> result = new();

        foreach (var building in _buildings)
        {
            foreach (var stack in building.inventory.currentStacks)
            {
                // 富余条件：超过最大值的60%，或者最大值为0视为无上限
                if (stack.storageLimit == 0 || stack.storageLimit > 0.6f * stack.storageLimit)
                {
                    result.Add((building, stack.Clone()));
                }
            }
        }

        return result;
    }


    /// <summary>
    /// 获取资源增长曲线（某个资源）
    /// </summary>
    public List<int> GetResourceHistory(ResourceConfig type)
    {
        List<int> output = new List<int>();
        foreach (var stack in resourceHistory.Values)
        {
            ResourceStack resourceStack = stack.FirstOrDefault(r => r.resourceConfig.Equals(type));
            if (resourceStack != null) output.Add(resourceStack.amount);
        }
        return output;
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
        if (building == null) return false;
        
        _buildings.Add(building);
        if(showDebugInfo)
            Debug.Log($"[BuildingManager] 建筑 {building.name} 已建造");
        
        OnBuildingBuilt?.Invoke(building);
        return true;
    }
    public bool BuildBuilding(BuildingSubType type, Vector2Int position) { return false; }
    public bool UpgradeBuilding(Building building) { return false; }
    public bool DestroyBuilding(Building building) { return false; }
    public bool RegisterBuilding(Building building)
    {
        if (building == null) return false;
        
        _buildings.Add(building);
        if(showDebugInfo)
            Debug.Log($"[BuildingManager] 建筑 {building.name} 已注册");
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
                var stack = building.inventory.currentStacks
                    .FirstOrDefault(r => r.resourceConfig.Equals(res));

                if (stack == null || stack.amount < stack.storageLimit)
                {
                    result.Add(building);
                    break;
                }
            }

            // 检查是否需要转移资源
            foreach (var stack in building.inventory.currentStacks)
            {
                // 如果资源超过最大值的60%，需要转移
                if (stack.storageLimit > 0 && stack.amount > 0.6f * stack.storageLimit)
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

        float weightSlot = 0.5f;
        float weightResource = 0.5f;
        List<(Building building, float score)> scored = new();
        foreach (var building in buildings)
        {
            // 缺人程度
            float slotRatio = 0f;
            if (building.maxSlotAmount > 0)
                slotRatio = (float)(building.maxSlotAmount - building.assignedNPCs.Count) / building.maxSlotAmount;

            // 缺资源程度（对所有AcceptResources取平均）
            float resourceRatio = 0f;
            int resourceCount = 0;
            float resourceSum = 0f;
            foreach (var res in building.AcceptResources)
            {
                var cur = building.inventory.currentStacks
                    .FirstOrDefault(r => r.resourceConfig.Equals(res));
                if (cur != null)
                {
                    resourceSum += 1f - (float)cur.amount / cur.storageLimit;
                    resourceCount++;
                }
            }
            if (resourceCount > 0)
                resourceRatio = resourceSum / resourceCount;

            float score = slotRatio * weightSlot + resourceRatio * weightResource;
            scored.Add((building, score));
        }
        if (scored.Count == 0) return null;
        float maxScore = scored.Max(x => x.score);
        var bestBuildings = scored.Where(x => Math.Abs(x.score - maxScore) < 0.0001f).Select(x => x.building).ToList();
        if (bestBuildings.Count == 1) return bestBuildings[0];
        // 多个得分相同，随机分配
        return bestBuildings[UnityEngine.Random.Range(0, bestBuildings.Count)];
    }

    // 资源变动事件处理
    public void OnBuildingResourceChanged(ResourceConfig config, int value)
    {
        // TODO: 这里可以根据value正负判断是产出还是消耗，动态维护resourceNeeds/resourceOutputs表
        // 这里只做Debug输出，后续可完善
        if (showDebugInfo)
            Debug.Log($"[BuildingManager] 资源变动: {config.type}-{config.subType} 变化量: {value}");
    }
}