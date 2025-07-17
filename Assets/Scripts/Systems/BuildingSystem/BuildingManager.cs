using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class BuildingManager : SingletonManager<BuildingManager>, ISaveable
{
    #region 字段声明
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    [Header("建筑配置")]
    [SerializeField] private BuildingConfig originalBuildingConfig;
    private BuildingConfig buildingConfig;
    [Header("建筑管理")]
    private List<Building> _buildings;
    private Dictionary<BuildingSubType, BuildingData> _buildingDataDict;
    private Dictionary<Vector2Int, Building> _buildingOccupies;
    private Dictionary<string, Building> _buildingsById; // 通过ID快速查找建筑

    [Header("资源管理")]
    // 历史资源变化记录（可用于曲线绘制）
    private Dictionary<DateTime, List<ResourceStack>> resourceHistory = new();
    // 资源需求与输出表
    private Dictionary<(ResourceType, int), List<Building>> resourceNeeds = new();
    private Dictionary<(ResourceType, int), List<Building>> resourceOutputs = new();

    [Header("Buff系统")]
    public Dictionary<BuildingSubType, Dictionary<BuffEnums, int>> AppliedBuffs;

    // 属性
    public BuildingConfig BuildingConfig{get{return buildingConfig;}}
    #endregion

    #region 事件声明
    public static event System.Action<Building> OnBuildingBuilt;
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action<Building> OnBuildingUpgraded;
    // public static event System.Action<Building> OnBuildingDestroyed;
    #endregion

    #region Unity生命周期
    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
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
    #endregion

    #region 初始化和设置
    /// <summary>
    /// 初始化BuildingManager
    /// </summary>
    public void Initialize()
    {
        // TODO: 后续改为从存档中读取
        _buildings = new List<Building>();
        _buildingDataDict = new Dictionary<BuildingSubType, BuildingData>();
        _buildingOccupies = new Dictionary<Vector2Int, Building>();
        _buildingsById = new Dictionary<string, Building>();
        AppliedBuffs = new Dictionary<BuildingSubType, Dictionary<BuffEnums, int>>();
        buildingConfig = Instantiate(originalBuildingConfig);
    }

    private void SubscribeEvents()
    {
        BuffBuilding.OnBuffBuildingBuilt += HandleBuffBuildingBuilt;
        BuffBuilding.OnBuffBuildingDestroyed += HandleBuffBuildingDestroyed;
        GameEvents.OnBuildingPlaced += HandleBuildingPlaced;
        GameEvents.OnBoughtBuildingPlacedAfterDragging += HandleBoughtBuildingPlacedAfterDragging;
        Debug.Log("[BuildingManager] Events subscribed successfully");
    }
    private void UnsubscribeEvents()
    {
        BuffBuilding.OnBuffBuildingBuilt -= HandleBuffBuildingBuilt;
        BuffBuilding.OnBuffBuildingDestroyed -= HandleBuffBuildingDestroyed;
        GameEvents.OnBuildingPlaced -= HandleBuildingPlaced;
        GameEvents.OnBoughtBuildingPlacedAfterDragging -= HandleBoughtBuildingPlacedAfterDragging;
        Debug.Log("[BuildingManager] Events unsubscribed successfully");
    }
    #endregion

    #region 资源管理
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

    /// <summary>
    /// 资源变动事件处理
    /// </summary>
    public void OnBuildingResourceChanged(ResourceConfig config, int value)
    {
        // TODO: 这里可以根据value正负判断是产出还是消耗，动态维护resourceNeeds/resourceOutputs表
        // 这里只做Debug输出，后续可完善
        if (showDebugInfo)
            Debug.Log($"[BuildingManager] 资源变动: {config.type}-{config.subType} 变化量: {value}");
    }
    #endregion

    #region Buff系统
    /// <summary>
    /// 处理Buff建筑建造事件
    /// </summary>
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

    /// <summary>
    /// 处理Buff建筑销毁事件
    /// </summary>
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
    #endregion

    #region 建筑管理
    /// <summary>
    /// 建筑建造完成，注册到管理器
    /// </summary>
    public bool BuildingBuilt(Building building)
    {
        if (building == null) return false;

        Debug.Log($"[BuildingManager] BuildingBuilt called for {building.name} (ID: {building.BuildingId})");

        _buildings.Add(building);
        _buildingsById[building.BuildingId] = building;
        if (showDebugInfo)
            Debug.Log($"[BuildingManager] 建筑 {building.name} (ID: {building.BuildingId}) 已建造");

        OnBuildingBuilt?.Invoke(building);
        return true;
    }

    /// <summary>
    /// 注册建筑到管理器
    /// </summary>
    public bool RegisterBuilding(Building building)
    {
        if (building == null) return false;
        if (_buildings.Contains(building))
        {
            Debug.LogWarning($"[BuildingManager] 建筑 {building.name} (ID: {building.BuildingId}) 已注册");
            return false;
        }else if(_buildingsById.ContainsKey(building.BuildingId))
        {
            Debug.LogWarning($"[BuildingManager] 建筑 {building.name} (ID: {building.BuildingId}) 已注册");
            return false;
        }
        _buildings.Add(building);
        _buildingsById[building.BuildingId] = building;
        if (showDebugInfo)
            Debug.Log($"[BuildingManager] 建筑 {building.name} (ID: {building.BuildingId}) 已注册");
        return true;
    }

    /// <summary>
    /// 从BuildingManager中移除建筑（当建筑被销毁时调用）
    /// </summary>
    public bool UnregisterBuilding(Building building)
    {
        if (building == null) return false;

        bool removedFromList = _buildings.Remove(building);
        bool removedFromDict = _buildingsById.Remove(building.BuildingId);

        if (showDebugInfo)
        {
            if (removedFromList || removedFromDict)
                Debug.Log($"[BuildingManager] 建筑 {building.name} (ID: {building.BuildingId}) 已从管理器移除");
            else
                Debug.LogWarning($"[BuildingManager] 尝试移除不存在的建筑 {building.name} (ID: {building.BuildingId})");
        }

        return removedFromList || removedFromDict;
    }

    /// <summary>
    /// 建造建筑（待实现）
    /// </summary>
    public bool BuildBuilding(BuildingSubType type, Vector2Int position) { return false; }
    public bool BuyBuilding(BuildingSubType type)
    {
        // 获取建筑数据
        BuildingData buildingData = GetBuildingData(type);
        if (buildingData == null)
        {
            if (showDebugInfo)
                Debug.LogError($"[BuildingManager] 建筑数据不存在: {type}");
            return false;
        }

        // 检查玩家是否拥有足够资源
        if (!ResourceManager.Instance.HasEnoughResource(ResourceManager.Instance.Gold, buildingData.purchasePrice))
        {
            if (showDebugInfo)
                Debug.LogWarning($"[BuildingManager] 玩家没有足够资源购买 {buildingData.buildingName}，需要 {buildingData.purchasePrice} 金币");
            GameEvents.TriggerResourceInsufficient(new ResourceEventArgs(){
                resourceType = ResourceType.Coin,
                subType = (int)CoinSubType.Gold,
                newAmount = buildingData.purchasePrice,
                timestamp = DateTime.Now
            });
            return false;
        }

        // 创建建筑实例
        var building = CreateBuilding(type);
        if (building == null)
        {
            if (showDebugInfo)
                Debug.LogError($"[BuildingManager] 创建建筑失败: {type}");
            return false;
        }

        // 触发建筑购买事件
        GameEvents.TriggerBuildingBought(new BuildingEventArgs()
        {
            building = building,
            eventType = BuildingEventArgs.BuildingEventType.Created,
            timestamp = DateTime.Now
        });
        return true;

    }
    public Building CreateBuilding(BuildingSubType type, List<Vector2Int> positions = null)
    {
        // 获取建筑预制体
        var prefab = buildingConfig.buildingPrefabDatas.Find(x => x.subType == type).prefab;
        if (prefab == null)
        {
            if (showDebugInfo)
                Debug.LogError($"[BuildingManager] 建筑预制体不存在: {type}");
            return null;
        }

        // 创建建筑实例
        Vector2Int position = new Vector2Int(99, 99); // 创建在一个很远的地方
        Vector3 worldPosition = new Vector3(position.x, 0, position.y);
        var instance = Instantiate(prefab, worldPosition, Quaternion.identity);

        // 获取建筑组件
        var building = instance.GetComponentInChildren<Building>();
        if (building == null)
        {
            if (showDebugInfo)
                Debug.LogError($"[BuildingManager] 在实体中没有找到 Building 组件: {type}");
            return null;
        }
        // 设置建筑数据
        building.SetBuildingData(GetBuildingOverallData(type));
        if (showDebugInfo)
            Debug.Log($"[BuildingManager] 建筑 {building.name} (ID: {building.BuildingId}) 已创建在 {worldPosition}");

        // 根据传入的positions，设置建筑位置
        if (positions != null)
        {
            if (showDebugInfo)
                Debug.Log($"[BuildingManager] 尝试根据读取到的位置信息将建筑 {building.name} (ID: {building.BuildingId}) 放置在 {positions.Count} 个位置");
            building.positions = positions; // 设置位置数据
            GameEvents.TriggerBuildingCreated(new BuildingEventArgs()
            {
                building = building,
                positions = positions,
                eventType = BuildingEventArgs.BuildingEventType.Created,
                timestamp = DateTime.Now
            });
            // // 根据building的位置信息将其放置在对应位置
            // building.PlaceableObject.PlaceAt(positions.Select(x => new Vector3Int(x.x, 0, x.y)).ToArray());
        }
        return building;
    }
    /// <summary>
    /// 升级建筑（待实现）
    /// </summary>
    public bool UpgradeBuilding(Building building) { return false; }

    /// <summary>
    /// 销毁建筑
    /// </summary>
    public bool DestroyBuilding(Building building)
    {
        if (building == null) return false;
        UnregisterBuilding(building);
        return building.DestroySelf();

    }
    /// <summary>
    /// 处理建筑放置事件,该事件负责处理拖拽事件
    /// </summary>
    public void HandleBoughtBuildingPlacedAfterDragging(BuildingEventArgs args)
    {
        if(args.building == null)
        {
            Debug.Log($"[BuildingManager] 建筑为空，判断为地皮放置，不做处理");
            return;
        }
        Debug.Log($"[BuildingManager] HandleBuildingPlaced called with event type: {args.eventType}, building: {args.building?.name}, timestamp: {args.timestamp}");

        if (args.eventType == BuildingEventArgs.BuildingEventType.PlaceSuccess)
        {
            // 注册建筑
            RegisterBuilding(args.building);
            // 消耗资源
            Debug.Log($"[BuildingManager] About to remove {args.building.data.purchasePrice} coins for building {args.building.name}");
            ResourceManager.Instance.RemoveResource(ResourceManager.Instance.Gold, args.building.data.purchasePrice);
            if (showDebugInfo)
                Debug.Log($"[BuildingManager] 建筑 {args.building.name} (ID: {args.building.BuildingId}) 已放置并注册, 消耗资源: {args.building.data.purchasePrice}");
        }
        else if (args.eventType == BuildingEventArgs.BuildingEventType.PlaceFailed)
        {
            // 销毁建筑
            DestroyBuilding(args.building);
        }
        else
        {
            Debug.LogError($"[BuildingManager] 建筑放置事件类型错误: {args.eventType}");
        }
    }
    private void HandleBuildingPlaced(BuildingEventArgs args)
    {

    }
    /// <summary>
    /// 根据subType从配置中获取建筑数据
    /// </summary>
    public BuildingPrefabData GetBuildingOverallData(BuildingSubType subType)
    {
        return buildingConfig.buildingPrefabDatas.Find(x => x.subType == subType);
    }
    /// <summary>
    /// 根据subType从配置中获取建筑基本数据
    /// </summary>
    public BuildingData GetBuildingData(BuildingSubType subType)
    {
        return buildingConfig.buildingPrefabDatas.Find(x => x.subType == subType).buildingDatas;
    }
    /// <summary>
    /// 根据subType从配置中获取建筑生产数据
    /// </summary>
    public ProductionBuildingData GetProductionBuildingData(BuildingSubType subType)
    {
        return buildingConfig.buildingPrefabDatas.Find(x => x.subType == subType).productionBuildingDatas;
    }
    public GameObject GetBuildingPrefab(BuildingSubType subType)
    {
        return buildingConfig.buildingPrefabDatas.Find(x => x.subType == subType).prefab;
    }
    #endregion

    #region 建筑查询
    /// <summary>
    /// 获取所有建筑
    /// </summary>
    public List<Building> GetAllBuildings() { return _buildings.ToList(); }

    /// <summary>
    /// 根据建筑类型获取建筑列表
    /// </summary>
    public List<Building> GetBuildingsByType(BuildingType type)
    {
        List<Building> buildingsReturn = new List<Building>();
        foreach (Building building in _buildings)
            if (building.data.buildingType == type)
                buildingsReturn.Add(building);
        return buildingsReturn;
    }

    /// <summary>
    /// 获取指定位置的建筑
    /// </summary>
    public Building GetBuildingAt(Vector2Int position) { return _buildingOccupies[position]; }

    /// <summary>
    /// 通过建筑ID查找建筑
    /// </summary>
    public Building GetBuildingById(string buildingId)
    {
        if (string.IsNullOrEmpty(buildingId)) return null;

        if (_buildingsById.TryGetValue(buildingId, out Building building))
        {
            return building;
        }

        if (showDebugInfo)
            Debug.LogWarning($"[BuildingManager] 未找到ID为 {buildingId} 的建筑");
        return null;
    }

    /// <summary>
    /// 检查建筑ID是否存在
    /// </summary>
    public bool HasBuildingWithId(string buildingId)
    {
        return !string.IsNullOrEmpty(buildingId) && _buildingsById.ContainsKey(buildingId);
    }

    /// <summary>
    /// 检查建筑是否解锁（待实现）
    /// </summary>
    public bool IsBuildingUnlocked(BuildingSubType type) { return false; }
    #endregion

    #region 工作系统
    /// <summary>
    /// 获取需要工作的建筑列表，按优先级排序
    /// </summary>
    public List<Building> GetBuildingsNeedingWork()
    {
        List<Building> result = new List<Building>();

        foreach (var building in _buildings)
        {
            // 检查是否有空余槽位
            if (building.assignedNPCs.Count < building.NPCSlotAmount)
            {
                result.Add(building);
                continue;
            }

            // 检查是否需要输入资源
            if (building.AcceptResources != null)
            {
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
            if (b.assignedNPCs.Count < b.NPCSlotAmount) return 0;
            if (b.AcceptResources.Any()) return 1;
            return 2;
        }).ToList();
    }

    /// <summary>
    /// 获取最适合NPC工作的建筑
    /// </summary>
    public TaskInfo GetBestWorkBuildingWorkForNPC(NPC npc)
    {
        Debug.Log("[Work] 开始查找最适合NPC的建筑物");
        var buildings = GetBuildingsNeedingWork();
        if (buildings.Count == 0) return TaskInfo.GetNone();

        List<(Building building, float score, TaskType workType)> scored = new();

        foreach (var building in buildings)
        {
            // 忽略住房与装饰性建筑
            if (building.data.buildingType == BuildingType.Housing
                || building.data.buildingType == BuildingType.Decoration) continue;

            // 读取权重
            var fitWeight = building.data.fitWeightData;
            float weightSlot = fitWeight.weightSlot;
            float weightResourceAgainst = fitWeight.weightResourceAgainst;
            float weightResourceInvolved = fitWeight.weightResourceInvolved;

            // 缺人程度
            float slotRatio = 0f;
            if (building.NPCSlotAmount > 0)
                slotRatio = (float)(building.NPCSlotAmount - building.assignedNPCs?.Count ?? 0) / building.NPCSlotAmount;

            float resourceRatioAgainst =
                building.inventory.GetResourceRatioLimitAgainstList(building.AcceptResources);
            float resourceRatioInvolving = building.inventory.GetResourceMappingWithFilter(npc.inventory, building.AcceptResources);

            float slotScore = slotRatio * weightSlot;
            float resourceAgainstScore = resourceRatioAgainst * weightResourceAgainst;
            float resourceInvolvingScore = resourceRatioInvolving * weightResourceInvolved;
            float score = slotScore + resourceAgainstScore + resourceInvolvingScore;

            Debug.Log($"[Work] 建筑: {building.data.subType} "
                      + $"| 插槽需求: {slotScore:F2} "
                      + $"| 资源需求: {resourceInvolvingScore:F2} "
                      + $"| 资源输出: {resourceAgainstScore:F2} "
                      + $"| 总分: {score:F2}");

            // 如果需求分数没有达到阈值就跳过
            if (score < 0.1f || !MathUtility.IsValid(score)) continue;
            Debug.Log($"[Work] 添加建筑 {building.data.subType} 需求分数：{score}");

            // 如果资源产出分数占比最高
            if (resourceAgainstScore > slotScore && resourceAgainstScore > resourceInvolvingScore)
                scored.Add((building, score, TaskType.HandlingAccept));
            // 如果插槽需求占比最高
            if (slotScore > resourceAgainstScore && slotScore > resourceInvolvingScore)
                scored.Add((building, score, TaskType.Production));
            // 如果资源需求分数占比最高
            if (resourceInvolvingScore > slotScore && resourceInvolvingScore > resourceAgainstScore)
                scored.Add((building, score, TaskType.HandlingDrop));
        }

        if (scored.Count == 0) return TaskInfo.GetNone();

        float maxScore = scored.Max(x => x.score);
        var bestBuildings = scored.Where(x => Math.Abs(x.score - maxScore) < 0.0001f)
                                 .Select(x => new TaskInfo(x.building, x.workType))
                                 .ToList();

        if (bestBuildings.Count == 1) return bestBuildings[0];

        // 多个得分相同，随机分配
        return bestBuildings[UnityEngine.Random.Range(0, bestBuildings.Count)];
    }
    #endregion

    #region 存档系统
    public GameSaveData GetSaveData()
    {
        List<BuildingInstanceSaveData> buildings = GetBuildingInstancesData();
        return new BuildingSaveData()
        {
            buildings = buildings
        };
    }

    public void LoadFromData(GameSaveData data)
    {
        BuildingSaveData buildingSaveData = data as BuildingSaveData;
        if (buildingSaveData == null)
        {
            Debug.LogError("[BuildingManager] LoadFromData: 数据转换失败，期望BuildingSaveData类型");
            return;
        }
        if (showDebugInfo)
        {
            Debug.Log($"[BuildingManager] 加载建筑数据: {buildingSaveData.buildings.Count}个建筑");
        }
        foreach (var buildingData in buildingSaveData.buildings)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BuildingManager] 加载建筑数据: {buildingData.buildingId} {buildingData.subType} {buildingData.status} {buildingData.currentLevel}");
            }
            // 创建建筑
            var building = CreateBuilding(buildingData.subType, buildingData.positions);
            // 加载建筑数据
            building.LoadFromData(buildingData);
        }
    }

    private List<BuildingInstanceSaveData> GetBuildingInstancesData()
    {
        return _buildings.Select(b => b.GetSaveData() as BuildingInstanceSaveData).ToList();
    }
    #endregion

    #region 调试
    [Button("打印所有建筑")]
    public void PrintAllBuildings()
    {
        foreach (var building in _buildings)
        {
            Debug.Log(building.data.buildingName + " " + building.BuildingId);
        }
    }
    #endregion
}