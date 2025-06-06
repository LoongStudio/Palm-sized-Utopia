using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class NPCManager : SingletonManager<NPCManager>
{
    [Header("社交系统")]
    [SerializeField] private bool enableSocialSystem = true;

    [SerializeField] private NPCGenerationConfig defaultNPCGenerationConfig;
    private List<NPC> allNPCs;
    private List<NPC> availableNPCs;
    public SocialSystem socialSystem;
    
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start(){
        // 订阅事件
        GameEvents.OnNPCInstantiated += OnNPCInstantiate;
    }
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
        // 加载社交系统配置文件
        var socialConfig = Resources.Load<SocialSystemConfig>("SocialSystemConfig");
        if (socialSystem != null)
        {
            socialSystem.Initialize(allNPCs, socialConfig);
        }
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
        
        // 避免重复注册
        if (allNPCs.Contains(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC {npc.data?.npcName} 已经被注册");
            return;
        }
        
        // 添加到总列表
        allNPCs.Add(npc);
        
        // 检查是否应该添加到可用列表
        if (IsNPCAvailable(npc))
        {
            availableNPCs.Add(npc);
        }
        
        Debug.Log($"[NPCManager] 注册NPC: {npc.data?.npcName}，当前总数: {allNPCs.Count}");
    }
    
    /// <summary>
    /// 从管理系统中移除NPC
    /// </summary>
    /// <param name="npc">要移除的NPC</param>
    public void UnregisterNPC(NPC npc)
    {
        if (npc == null) return;
        
        allNPCs.Remove(npc);
        availableNPCs.Remove(npc);
        
        Debug.Log($"[NPCManager] 移除NPC: {npc.data?.npcName}，剩余总数: {allNPCs.Count}");
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
    public bool HireNPC(NPCData npcData = null) {
        // 如果没有传入NPCData，则生成一个随机NPC
        if (npcData == null){
            npcData = GenerateRandomNPCData();
        }

        Debug.Log($"[NPCManager] 尝试雇佣NPC: {npcData.npcName}");

        // 雇佣NPC事件
        var eventArgs = new NPCEventArgs(){
            npcData = npcData,
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
    
}