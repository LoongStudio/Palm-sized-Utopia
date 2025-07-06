using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.AI;
using Sirenix.OdinInspector;

public class NPCManager : SingletonManager<NPCManager>, ISaveable
{
    [Header("调试信息")]
    [SerializeField] public bool showDebugInfo = false;
    [Header("社交系统")]
    [SerializeField] private bool enableSocialSystem = true;
    [SerializeField] private SocialSystemConfig defaultSocialSystemConfig;

    [SerializeField] private NPCGenerationConfig defaultNPCGenerationConfig;
    [Header("工作系统")]
    [SerializeField] private bool enableWorkSystem = true;

    private List<NPC> allNPCs;
    private List<NPC> availableNPCs;
    // ID到NPC的映射表
    private Dictionary<string, NPC> npcIdToNpcMap = new Dictionary<string, NPC>();
    // 用于检测ID冲突
    private HashSet<string> usedNpcIds = new HashSet<string>();
    private List<NPCInstanceSaveData> npcInstanceDataCache = new List<NPCInstanceSaveData>();
    public SocialSystem socialSystem;
    #region 生命周期
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start(){
        // 订阅事件
        SubscribeToEvents();
    }
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    #endregion

    #region 初始化
    public void Initialize() 
    { 
        allNPCs = new List<NPC>();
        availableNPCs = new List<NPC>();

        if (enableSocialSystem && socialSystem == null)
        {
            socialSystem = new SocialSystem();
        }
        // 等待一帧确保所有NPC都已初始化
        StartCoroutine(DelayedInit());

        // TODO: 订阅游戏事件
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitForEndOfFrame();

        // 1. 收集场景中已存在的NPC
        CollectExistingNPCs();

        // 2. 从存档加载NPC（如果有的话）
        // TODO: 从存档加载NPC

        // 3. 初始化社交系统
        InitializeSocialSystem();

        if(showDebugInfo) 
            Debug.Log($"[NPCManager] 初始化完成，管理 {allNPCs.Count} 个NPC");
    }

    /// <summary>
    /// 收集场景中已存在的NPC
    /// </summary>
    private void CollectExistingNPCs()
    {
        var sceneNPCs = FindObjectsByType<NPC>(FindObjectsSortMode.None).ToList();
        
        foreach (var npc in sceneNPCs)
        {
            RegisterNPC(npc);
        }
        
        if(showDebugInfo) 
            Debug.Log($"[NPCManager] 从场景中收集到 {sceneNPCs.Count} 个NPC");
    }

    /// <summary>
    /// 从存档加载NPC数据
    /// </summary>
    private void LoadNPCsFromSave()
    {
        // TODO: 实现从存档加载NPC的逻辑
        // 这里可能需要从SaveManager获取数据并实例化NPC
        
        /*
        var saveData = SaveManager.Instance.GetNPCData();
        if (saveData != null)
        {
            foreach (var npcData in saveData.npcs)
            {
                var npc = CreateNPCFromData(npcData);
                RegisterNPC(npc);
            }
        }
        */
    }
    /// <summary>
    /// 初始化社交系统
    /// </summary>
    private void InitializeSocialSystem()
    {

        if (socialSystem != null)
        {
            socialSystem.Initialize(allNPCs, defaultSocialSystemConfig);
        }
    }
    #endregion

    #region 事件订阅和取消订阅
    private void SubscribeToEvents()
    {
        GameEvents.OnNPCInstantiated += OnNPCInstantiate;
        GameEvents.OnNPCCreatedFromList += OnNPCCreatedFromList;
    }
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnNPCInstantiated -= OnNPCInstantiate;
        GameEvents.OnNPCCreatedFromList -= OnNPCCreatedFromList;
    }
    #endregion

    #region 事件处理
    private void OnNPCCreatedFromList(NPCEventArgs args)
    {
        // 事件验证
        if (args.eventType != NPCEventArgs.NPCEventType.CreatedFromList)
        {
            Debug.LogWarning("[NPCManager] 收到的事件类型不正确");
            return;
        }
        // 数据验证
        var npcList = args.npcList;
        if (npcList == null || npcList.Count == 0)
        {
            Debug.LogWarning("[NPCManager] 收到空的NPC列表");
            return;
        }

        if (npcList.Count != npcInstanceDataCache.Count)
        {
            Debug.LogError("[NPCManager] 收到的加载完成事件中的NPC数量与保存数据缓存中的NPC数量不一致");
        }

        // 对收到的列表中的NPC应用保存数据
        for (int i = 0; i < npcList.Count; i++)
        {
            var npc = npcList[i];
            var npcInstanceData = npcInstanceDataCache[i];
            npc.LoadFromData(npcInstanceData);
        }

        // 清理临时数据缓存
        npcInstanceDataCache.Clear();
    }
    #endregion

    #region NPC注册和可用判断
    
    /// <summary>
    /// 注册NPC到管理系统
    /// </summary>
    /// <param name="npc">要注册的NPC</param>
    public void RegisterNPC(NPC npc)
    {
        if (npc == null)
        {
            Debug.LogError("[NPCManager] 尝试注册空的NPC");
            return;
        }

        // 检查ID冲突
        string npcId = npc.NpcId;
        if (usedNpcIds.Contains(npcId))
        {
            Debug.LogWarning($"[NPCManager] 检测到NPC ID冲突: {npcId}，重新生成ID");
            npc.RegenerateId();
            npcId = npc.NpcId;
        }
        
        // 避免重复注册
        if (allNPCs.Contains(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC {npc.data?.npcName} 已经被注册");
            return;
        }
        
        // 添加到总列表
        allNPCs.Add(npc);
        npcIdToNpcMap[npcId] = npc;
        usedNpcIds.Add(npcId);        
        
        // 检查是否应该添加到可用列表
        if (IsNPCAvailable(npc))
        {
            availableNPCs.Add(npc);
        }
        
        if(showDebugInfo) 
            Debug.Log($"[NPCManager] 注册NPC: {npc.data?.npcName}，当前总数: {allNPCs.Count}");
    }
    
    /// <summary>
    /// 从管理系统中移除NPC
    /// </summary>
    /// <param name="npc">要移除的NPC</param>
    public void UnregisterNPC(NPC npc)
    {
        if (npc == null) return;

        string npcId = npc.NpcId;
        
        allNPCs.Remove(npc);
        availableNPCs.Remove(npc);
        npcIdToNpcMap.Remove(npcId);
        usedNpcIds.Remove(npcId);

        // 清理社交关系
        if (socialSystem != null)
        {
            socialSystem.RemoveAllRelationshipsFor(npc);
        }
        
        if(showDebugInfo) 
            Debug.Log($"[NPCManager] 移除NPC: {npc.data?.npcName}，剩余总数: {allNPCs.Count}");
    }

    /// <summary>
    /// 通过ID获取NPC
    /// </summary>
    public NPC GetNPCById(string npcId)
    {
        if (string.IsNullOrEmpty(npcId))
            return null;
            
        npcIdToNpcMap.TryGetValue(npcId, out var npc);
        return npc;
    }
    
    /// <summary>
    /// 检查ID是否已被使用
    /// </summary>
    public bool IsNpcIdUsed(string npcId)
    {
        return usedNpcIds.Contains(npcId);
    }
    
    /// <summary>
    /// 获取所有NPC的ID列表
    /// </summary>
    public List<string> GetAllNpcIds()
    {
        return new List<string>(usedNpcIds);
    }
    
    /// <summary>
    /// 验证所有NPC的ID完整性
    /// </summary>
    [ContextMenu("验证NPC ID完整性")]
    public void ValidateNpcIds()
    {
        Debug.Log("[NPCManager] 开始验证NPC ID完整性...");
        
        var foundIssues = new List<string>();
        
        // 检查重复ID
        var idCounts = new Dictionary<string, int>();
        foreach (var npc in allNPCs)
        {
            if (npc != null)
            {
                string id = npc.NpcId;
                idCounts[id] = idCounts.ContainsKey(id) ? idCounts[id] + 1 : 1;
            }
        }
        
        foreach (var kvp in idCounts)
        {
            if (kvp.Value > 1)
            {
                foundIssues.Add($"重复ID: {kvp.Key} (出现{kvp.Value}次)");
            }
        }
        
        // 检查映射表一致性
        if (npcIdToNpcMap.Count != allNPCs.Count)
        {
            foundIssues.Add($"映射表数量不一致: 映射表{npcIdToNpcMap.Count}, 实际NPC{allNPCs.Count}");
        }
        
        // 检查每个NPC是否在映射表中
        foreach (var npc in allNPCs)
        {
            if (npc != null && !npcIdToNpcMap.ContainsKey(npc.NpcId))
            {
                foundIssues.Add($"NPC {npc.data?.npcName} (ID: {npc.NpcId}) 不在映射表中");
            }
        }
        
        if (foundIssues.Count == 0)
        {
            Debug.Log("[NPCManager] ✅ NPC ID完整性验证通过");
        }
        else
        {
            Debug.LogError($"[NPCManager] ❌ 发现{foundIssues.Count}个问题:");
            foreach (var issue in foundIssues)
            {
                Debug.LogError($"  - {issue}");
            }
        }
    }
    
    /// <summary>
    /// 判断NPC是否可用（可以被分配工作）
    /// </summary>
    private bool IsNPCAvailable(NPC npc)
    {
        // TODO: 判断NPC是否可用
        return true;
    }
    
    #endregion

    #region NPC雇佣和解雇
    public bool HireNPC(NPCData npcData = null, InventorySaveData inventorySaveData = null) {
        // 如果没有传入NPCData，则生成一个随机NPC
        if (npcData == null){
            npcData = GenerateRandomNPCData();
        }

        if(showDebugInfo) 
            Debug.Log($"[NPCManager] 尝试雇佣NPC: {npcData.npcName}, ID: {npcData.npcId}");

        // 雇佣NPC事件
        var eventArgs = new NPCEventArgs(){
            npcData = npcData,
            inventorySaveData = inventorySaveData,
            eventType = NPCEventArgs.NPCEventType.Hired,
            timestamp = System.DateTime.Now
        };
        GameEvents.TriggerNPCHired(eventArgs);

        // 返回雇佣结果
        return true;
    }
    
    private void OnNPCInstantiate(NPCEventArgs args){
        // 注册NPC
        if(args.npc != null){
            RegisterNPC(args.npc);
            if(showDebugInfo) 
                Debug.Log($"[NPCManager] 雇佣NPC成功: {args.npc.data.npcName}");
        } else{
            Debug.LogError("[NPCManager] 雇佣NPC失败，原因：NPC实例化失败");
        }
    }
    public bool FireNPC(NPC npc) { return false; }

    #endregion

    #region 私有NPCData生成方法
    /// <summary>
    /// 核心方法：生成随机NPCData
    /// </summary>
    /// <param name="config">NPC生成配置，如果为空则使用默认配置</param>
    /// <returns>生成的NPCData</returns>
    private NPCData GenerateRandomNPCData(NPCGenerationConfig config = null) 
    {
        var activeConfig = config ?? defaultNPCGenerationConfig;

        if (activeConfig == null)
        {
            Debug.LogError("[NPCManager] 没有配置文件，无法生成NPC");
            return null;
        }

        NPCData npcData = new NPCData();

        // 生成基础属性
        npcData.npcName = GenerateRandomName(activeConfig);
        npcData.baseSalary = GenerateRandomValue(activeConfig.SalaryRange);
        npcData.baseWorkAbility = GenerateRandomValue(activeConfig.BaseWorkAbilityRange);
        npcData.itemCapacity = GenerateRandomValue(activeConfig.ItemCapacityRange);
        npcData.itemTakeEachTimeCapacity = GenerateRandomValue(activeConfig.ItemTakeEachTimeCapacityRange);
        
        // 生成时间属性 - 确保时间逻辑合理
        npcData.restTimeStart = GenerateRandomValue(activeConfig.RestStartHourRange);
        npcData.restTimeEnd = GenerateRandomValue(activeConfig.RestEndHourRange);
        npcData.workTimeStart = GenerateRandomValue(activeConfig.WorkStartHourRange);
        npcData.workTimeEnd = GenerateRandomValue(activeConfig.WorkEndHourRange);
        
        // 验证时间设置的合理性
        ValidateTimeSettings(npcData);
        
        // 生成性格
        npcData.personality = GenerateRandomPersonality(activeConfig);
        
        // 生成词条
        npcData.traits = GenerateRandomTraits(activeConfig);
        
        if(showDebugInfo) 
            Debug.Log($"[NPCManager] 生成了新NPC: {npcData.npcName} - 性格:{npcData.personality} - 词条数量:{npcData.traits.Count}");
        return npcData;
    }

    private void ValidateTimeSettings(NPCData npcData)
    {
        // 确保休息时间不会与工作时间冲突
        // 如果休息开始时间早于结束时间，说明是跨夜休息
        bool isOvernightRest = npcData.restTimeStart > npcData.restTimeEnd;
        
        if (!isOvernightRest)
        {
            // 同一天的休息时间，确保工作时间不在休息时间内
            if (npcData.workTimeStart >= npcData.restTimeStart && npcData.workTimeStart <= npcData.restTimeEnd)
            {
                npcData.workTimeStart = npcData.restTimeEnd + 1;
            }
            if (npcData.workTimeEnd >= npcData.restTimeStart && npcData.workTimeEnd <= npcData.restTimeEnd)
            {
                npcData.workTimeEnd = npcData.restTimeStart - 1;
            }
        }
        
        // 确保工作开始时间早于结束时间
        if (npcData.workTimeStart >= npcData.workTimeEnd)
        {
            npcData.workTimeStart = Mathf.Max(9, npcData.workTimeEnd - 8); // 至少工作8小时
        }
    }
    private string GenerateRandomName(NPCGenerationConfig config)
    {
        if (config.FirstNames.Length == 0 || config.LastNames.Length == 0)
        {
            return $"NPC_{UnityEngine.Random.Range(1000, 9999)}";
        }
        
        string firstName = config.FirstNames[UnityEngine.Random.Range(0, config.FirstNames.Length)];
        string lastName = config.LastNames[UnityEngine.Random.Range(0, config.LastNames.Length)];
        
        return $"{lastName}{firstName}";
    }

    private int GenerateRandomValue(Vector2Int range, bool useGaussianDistribution = false)
    {
        if (useGaussianDistribution)
        {
            return GenerateGaussianValue(range);
        }
        else
        {
            return UnityEngine.Random.Range(range.x, range.y + 1);
        }
    }

    private int GenerateGaussianValue(Vector2Int range, float gaussianDeviation = 0.3f)
    {
        float mean = (range.x + range.y) / 2f;
        float stdDev = (range.y - range.x) * gaussianDeviation;
        
        float value = SampleGaussian(mean, stdDev);
        return Mathf.Clamp(Mathf.RoundToInt(value), range.x, range.y);
    }

    private float SampleGaussian(float mean, float stdDev)
    {
        // Box-Muller变换生成正态分布
        float u1 = 1.0f - UnityEngine.Random.value;
        float u2 = 1.0f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
    
    private NPCPersonalityType GenerateRandomPersonality(NPCGenerationConfig config)
    {
        return WeightedRandomSelect(config.PersonalityWeights, w => w.personality, w => w.weight);
    }

    private List<NPCTraitType> GenerateRandomTraits(NPCGenerationConfig config)
    {
        int traitCount = GenerateRandomValue(config.TraitCountRange);
        List<NPCTraitType> selectedTraits = new List<NPCTraitType>();
        
        // 避免重复选择同一个词条
        List<TraitWeight> availableTraits = new List<TraitWeight>(config.TraitWeights);
        
        for (int i = 0; i < traitCount && availableTraits.Count > 0; i++)
        {
            NPCTraitType selectedTrait = WeightedRandomSelect(availableTraits.ToArray(), w => w.trait, w => w.weight);
            selectedTraits.Add(selectedTrait);
            
            // 移除已选择的词条，避免重复
            availableTraits.RemoveAll(t => t.trait == selectedTrait);
        }
        
        return selectedTraits;
    }
    
    private T WeightedRandomSelect<T, W>(W[] weights, System.Func<W, T> valueSelector, System.Func<W, float> weightSelector)
    {
        if (weights.Length == 0)
        {
            Debug.LogError("[NPCManager] WeightedRandomSelect: 权重数组为空");
            return default(T);
        }
        
        float totalWeight = 0f;
        foreach (var weight in weights)
        {
            totalWeight += weightSelector(weight);
        }
        
        if (totalWeight <= 0f)
        {
            Debug.LogWarning("[NPCManager] WeightedRandomSelect: 总权重为0，返回第一个元素");
            return valueSelector(weights[0]);
        }
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var weight in weights)
        {
            currentWeight += weightSelector(weight);
            if (randomValue <= currentWeight)
            {
                return valueSelector(weight);
            }
        }
        
        // 如果没有选中任何项（不应该发生），返回第一个
        return valueSelector(weights[0]);
    }
    #endregion

    #region 保存与加载
    public GameSaveData GetSaveData()
    {
        // 所有NPC的实例数据
        List<NPCInstanceSaveData> npcInstancesData = GetNPCInstancesData();
        // 社交系统数据
        SocialSystemSaveData socialSystemData = GetSocialSystemData();

        return new NPCSaveData{
            npcInstances = npcInstancesData,
            socialSystemSaveData = socialSystemData
        };
    }
    public void LoadFromData(GameSaveData data) {
        var npcSaveData = data as NPCSaveData;
        if (npcSaveData != null)
        {
            // 加载NPC数据到自身
            LoadNPCsFrom(npcSaveData.npcInstances);
            // 将社交系统数据加载到社交系统
            int count = npcSaveData.npcInstances.Count;
            StartCoroutine(LoadSocialSystemData(count,npcSaveData.socialSystemSaveData));
        }
        else
        {
            Debug.LogError("[NPCManager] LoadFromData: 数据转换失败，期望NPCSaveData类型");
        }
    }
    /// <summary>             
    /// 从现有NPC获取所有NPC的实例数据
    /// </summary>
    private List<NPCInstanceSaveData> GetNPCInstancesData()
    {
        return allNPCs.Select(npc => new NPCInstanceSaveData
        {
            npcId = npc.NpcId,
            npcData = npc.data,
            inventorySaveData = npc.inventory.GetSaveData() as InventorySaveData
        }).ToList();
    }
    /// <summary>
    /// 从社交系统获取社交系统数据
    /// </summary>
    private SocialSystemSaveData GetSocialSystemData()
    {
        return socialSystem.GetSaveData() as SocialSystemSaveData;
    }
    /// <summary>
    /// 从存档数据加载NPC并注册生成到场景中
    /// </summary>
    private void LoadNPCsFrom(List<NPCInstanceSaveData> npcInstancesData)
    {
        // 触发事件：NPC存储数据加载事件
        var eventArgs = new NPCEventArgs() {
            npcInstancesList = npcInstancesData,
            eventType = NPCEventArgs.NPCEventType.LoadedFromData,
            timestamp = System.DateTime.Now
        };
        GameEvents.TriggerNPCLoadedFromData(eventArgs);
        // 将读取到的数据临时保存
        npcInstanceDataCache = npcInstancesData;
        // foreach (var npcInstanceData in npcInstancesData)
        // {

        //     // 注册并生成NPC到场景中
        //     HireNPC(npcInstanceData.npcData, npcInstanceData.inventorySaveData);
        // }
    }

    /// <summary>
    /// 延迟加载社交系统数据，确保所有NPC都注册完成
    /// </summary>
    private IEnumerator LoadSocialSystemData(int npcCount,SocialSystemSaveData socialSystemSaveData)
    {
        // 记录开始的时间
        float startTime = Time.time;
        // 等待直到全部读取到的NPC已经注册完成
        while (allNPCs.Count < npcCount)
        {
            if(Time.time - startTime > 3f){
                Debug.LogError("[NPCManager] 加载社交系统数据超过3秒，加载失败， 因为有NPC没有注册成功。");
                // 终止该协程
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
            
        // 将社交系统数据加载到社交系统
        socialSystem.LoadFromData(socialSystemSaveData);
    }
    #endregion

    public bool AssignNPCToBuilding(NPC npc, Building building) { return false; }
    public void RemoveNPCFromBuilding(NPC npc) { }
    
    // 查询方法
    public List<NPC> GetAllNPCs() {
        return allNPCs;
    }
    public List<NPC> GetAvailableNPCs() {
        return availableNPCs;
    }
    public List<NPC> GetWorkingNPCs() {
        return allNPCs.Where(npc => npc.currentState == NPCState.Working).ToList();
    }
    
    // 工资系统
    public void PaySalaries() { }
    public int GetTotalSalaryCost() { return 0; }
    
    private void Update()
    {
        UpdateNPCStates();
        socialSystem.UpdateSocialInteractions();
    }
    
    private void UpdateNPCStates() { }

    [ContextMenu("Test Generate NPC Data")]
    public void TestGenerateNPC()
    {
        var npcData = GenerateRandomNPCData();
        Debug.Log($"[NPCManager] 生成了新NPC: {npcData}");
    }

    [ContextMenu("Test Hire NPC")]
    public void TestHireNPC()
    {
        if(Application.isPlaying    ){
            HireNPC();
        } else{
            Debug.LogError("[NPCManager] 请在运行时调用TestHireNPC");
        }
    }
    [Button("打印所有NPC")]
    public void PrintAllNPCs()
    {
        Debug.Log($"[NPCManager] ========================== 输出所有NPC ==========================");
        foreach (var npc in allNPCs)
        {
            Debug.Log($"[NPCManager] NPC: {npc.data.npcName}, NPCId: {npc.NpcId}");
        }
        Debug.Log($"[NPCManager] ========================== 输出所有NPC ==========================");
    }
    [Button("删除所有NPC")]
    public void DeleteAllNPCs()
    {
        // 注意不能在foreach中删除遍历的元素
        var npcList = FindObjectsByType<NPC>(FindObjectsSortMode.None).ToList();
        foreach (var npc in npcList)
        {
            DestroyNPC(npc);
        }
    }

    [Button("打印所有活跃互动")]
    public void PrintActiveInteractions()
    {
        Debug.Log($"[NPCManager] ========================== 输出所有活跃互动 ==========================");
        foreach (var interaction in socialSystem.activeInteractions)
        {
            Debug.Log($"[NPCManager] NPC: {interaction.Key.Item1.data.npcName} 和 NPC: {interaction.Key.Item2.data.npcName} 正在互动，该互动应当持续时间: {interaction.Value.duration}秒，已进行时间: {interaction.Value.elapsed}秒，剩余时间: {interaction.Value.duration - interaction.Value.elapsed}秒");    
        }
        Debug.Log($"[NPCManager] ========================== 输出所有活跃互动 ==========================");
    }

    [Button("打印所有互动冷却时间")]
    public void PrintInteractionCooldowns()
    {
        Debug.Log($"[NPCManager] ========================== 输出所有互动冷却时间 ==========================");
        foreach (var cooldown in socialSystem.interactionCooldowns)
        {
            Debug.Log($"[NPCManager] NPC: {cooldown.Key.Item1.data.npcName} 和 NPC: {cooldown.Key.Item2.data.npcName} 的互动冷却时间: {cooldown.Value}");
        }
        Debug.Log($"[NPCManager] ========================== 输出所有互动冷却时间 ==========================");
    }

    [Button("打印所有每日互动计数")]
    public void PrintDailyInteractionCounts()
    {
        Debug.Log($"[NPCManager] ========================== 输出所有每日互动计数 ==========================");
        foreach (var count in socialSystem.dailyInteractionCounts)
        {
            Debug.Log($"[NPCManager] NPC: {count.Key.data.npcName} 的每日互动计数: {count.Value}");
        }
        Debug.Log($"[NPCManager] ========================== 输出所有每日互动计数 ==========================");
    }
    

    // 社交位置计算相关方法已移动到SocialSystem中

    /// <summary>
    /// 安全地销毁NPC，确保从管理系统中正确移除
    /// </summary>
    /// <param name="npc">要销毁的NPC</param>
    public void DestroyNPC(NPC npc)
    {
        if (npc == null) return;
        
        if(showDebugInfo) 
            Debug.Log($"[NPCManager] 安全销毁NPC: {npc.data?.npcName}");
        
        // 先从管理系统中移除
        UnregisterNPC(npc);
        
        // 如果NPC有住房，则从住房中移除
        if (npc.housing != null)
        {
            npc.housing.UnRegisterLivingNPC(npc);
        }
        
        // 然后销毁GameObject
        if (npc.gameObject != null)
        {
            Object.Destroy(npc.gameObject);
        }
    }
}

// SocialPositions结构体已移动到SocialSystem中