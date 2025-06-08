using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.AI;

public class NPCManager : SingletonManager<NPCManager>
{
    [Header("社交系统")]
    [SerializeField] private bool enableSocialSystem = true;

    [SerializeField] private NPCGenerationConfig defaultNPCGenerationConfig;
    private List<NPC> allNPCs;
    private List<NPC> availableNPCs;
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

    #region 事件订阅和取消订阅
    private void SubscribeToEvents()
    {
        GameEvents.OnNPCInstantiated += OnNPCInstantiate;
        GameEvents.OnNPCShouldStartSocialInteraction += HandleNPCShouldStartSocialInteraction;
        GameEvents.OnNPCSocialInteractionEnded += HandleSocialInteractionEnded;
    }
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnNPCInstantiated -= OnNPCInstantiate;
        GameEvents.OnNPCShouldStartSocialInteraction -= HandleNPCShouldStartSocialInteraction;
        GameEvents.OnNPCSocialInteractionEnded -= HandleSocialInteractionEnded;
    }
    #endregion

    #region 事件处理
    private void HandleNPCShouldStartSocialInteraction(NPCEventArgs args)
    {
        if (args.npc == null || args.otherNPC == null)
        {
            Debug.LogWarning("[NPCManager] 社交事件中的NPC为空");
            return;
        }

        Debug.Log($"[NPCManager] NPC {args.npc.data.npcName} 和NPC {args.otherNPC.data.npcName} 准备开始社交互动");
        NPC npc1 = args.npc;
        NPC npc2 = args.otherNPC;

        // 验证两个NPC是否在管理列表中
        if (!allNPCs.Contains(npc1) || !allNPCs.Contains(npc2))
        {
            Debug.LogError($"[NPCManager] NPC {npc1.data.npcName} 或 NPC {npc2.data.npcName} 不在管理列表中，无法处理社交互动");
            return;
        }

        StartCoroutine(ExecutePrepareForSocialInteraction(npc1, npc2));
    }

    private IEnumerator ExecutePrepareForSocialInteraction(NPC npc1, NPC npc2)
    {
        // 阶段0: 修改NPC状态
        npc1.currentState = NPCState.MovingToSocial;
        npc2.currentState = NPCState.MovingToSocial;
        
        // 阶段1: 计算社交位置
        var socialPositions = CalculateSocialPositions(npc1, npc2);
        Debug.Log($"[NPCManager] 计算社交位置为: {socialPositions.npc1Position} 和 {socialPositions.npc2Position}");

        // 阶段2: 让两个NPC移动到社交位置
        yield return StartCoroutine(MoveNPCsToSocialPositions(npc1, npc2, socialPositions));

    }

    private void HandleSocialInteractionEnded(NPCEventArgs args){

        if (args.npc == null || args.otherNPC == null)
        {
            Debug.LogWarning("[NPCManager] 社交事件中的NPC为空");
            return;
        }
        Debug.Log($"[NPCManager] NPC {args.npc.data.npcName} 和NPC {args.otherNPC.data.npcName} 结束社交互动");
        NPC npc1 = args.npc;
        NPC npc2 = args.otherNPC;

        // 验证两个NPC是否在管理列表中
        if(!allNPCs.Contains(npc1) || !allNPCs.Contains(npc2)){
            Debug.LogError($"[NPCManager] NPC {npc1.data.npcName} 或 NPC {npc2.data.npcName} 不在管理列表中，无法处理社交互动");
            return;
        }

        // TODO：NPC的社交互动结束后的逻辑
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
    [ContextMenu("Print All NPCs")]
    public void PrintAllNPCs()
    {
        Debug.Log($"[NPCManager] ========================== 输出所有NPC ==========================");
        foreach (var npc in allNPCs)
        {
            Debug.Log($"[NPCManager] NPC: {npc.data.npcName}");
        }
        Debug.Log($"[NPCManager] ========================== 输出所有NPC ==========================");
    }

    [ContextMenu("Print Active Interactions")]
    public void PrintActiveInteractions()
    {
        Debug.Log($"[NPCManager] ========================== 输出所有活跃互动 ==========================");
        foreach (var interaction in socialSystem.activeInteractions)
        {
            Debug.Log($"[NPCManager] NPC: {interaction.Key.Item1.data.npcName} 和 NPC: {interaction.Key.Item2.data.npcName} 正在互动，该互动应当持续时间: {interaction.Value.duration}秒，已进行时间: {interaction.Value.elapsed}秒，剩余时间: {interaction.Value.duration - interaction.Value.elapsed}秒");    
        }
        Debug.Log($"[NPCManager] ========================== 输出所有活跃互动 ==========================");
    }

    [ContextMenu("Print Interaction Cooldowns")]
    public void PrintInteractionCooldowns()
    {
        Debug.Log($"[NPCManager] ========================== 输出所有互动冷却时间 ==========================");
        foreach (var cooldown in socialSystem.interactionCooldowns)
        {
            Debug.Log($"[NPCManager] NPC: {cooldown.Key.Item1.data.npcName} 和 NPC: {cooldown.Key.Item2.data.npcName} 的互动冷却时间: {cooldown.Value}");
        }
        Debug.Log($"[NPCManager] ========================== 输出所有互动冷却时间 ==========================");
    }

    [ContextMenu("Print Daily Interaction Counts")]
    public void PrintDailyInteractionCounts()
    {
        Debug.Log($"[NPCManager] ========================== 输出所有每日互动计数 ==========================");
        foreach (var count in socialSystem.dailyInteractionCounts)
        {
            Debug.Log($"[NPCManager] NPC: {count.Key.data.npcName} 的每日互动计数: {count.Value}");
        }
        Debug.Log($"[NPCManager] ========================== 输出所有每日互动计数 ==========================");
    }
    

    /// <summary>
    /// 计算两个NPC的社交位置
    /// </summary>
    private SocialPositions CalculateSocialPositions(NPC npc1, NPC npc2)
    {
        Vector3 npc1Pos = npc1.transform.position;
        Vector3 npc2Pos = npc2.transform.position;
        
        // 计算中点作为社交中心
        Vector3 socialCenter = (npc1Pos + npc2Pos) * 0.5f;
        
        // 计算两个NPC之间的方向
        Vector3 direction = (npc2Pos - npc1Pos).normalized;
        
        // 确保社交位置在NavMesh上
        socialCenter = FindNearestNavMeshPoint(socialCenter);
        
        // 计算两个NPC的最终位置（面对面，保持socialInteractionDistance的距离）
        float halfDistance = socialSystem.socialInteractionDistance * 0.5f;
        Vector3 npc1TargetPos = socialCenter - direction * halfDistance;
        Vector3 npc2TargetPos = socialCenter + direction * halfDistance;
        
        // 确保目标位置在NavMesh上
        npc1TargetPos = FindNearestNavMeshPoint(npc1TargetPos);
        npc2TargetPos = FindNearestNavMeshPoint(npc2TargetPos);
        
        return new SocialPositions
        {
            npc1Position = npc1TargetPos,
            npc2Position = npc2TargetPos,
            socialCenter = socialCenter,
            facingDirection = direction
        };
    }

    /// <summary>
    /// 找到最近的NavMesh点
    /// </summary>
    private Vector3 FindNearestNavMeshPoint(Vector3 position)
    {
        if (UnityEngine.AI.NavMesh.SamplePosition(position, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }
        return position; // 如果找不到，返回原位置
    }

        /// <summary>
    /// 让两个NPC移动到社交位置
    /// </summary>
    private IEnumerator MoveNPCsToSocialPositions(NPC npc1, NPC npc2, SocialPositions positions)
    {
        Debug.Log($"[NPCManager] 开始移动两个NPC到社交位置: {positions.npc1Position} 和 {positions.npc2Position}");
        // 启动两个NPC的社交移动
        var moveCoroutine1 = StartCoroutine(npc1.MoveToSocialPosition(positions.npc1Position, socialSystem.socialMoveSpeed));
        var moveCoroutine2 = StartCoroutine(npc2.MoveToSocialPosition(positions.npc2Position, socialSystem.socialMoveSpeed));
        
        // 等待两个移动完成，或者超时
        float startTime = Time.time;
        bool npc1Arrived = false;
        bool npc2Arrived = false;
        
        while ((!npc1Arrived || !npc2Arrived) && (Time.time - startTime) < socialSystem.socialTimeout)
        {
            // 检查NPC1是否到达
            if (!npc1Arrived)
            {
                
                npc1Arrived = npc1.navAgent.remainingDistance <= npc1.navAgent.stoppingDistance;
            }
            
            // 检查NPC2是否到达
            if (!npc2Arrived)
            {
                npc2Arrived = npc2.navAgent.remainingDistance <= npc2.navAgent.stoppingDistance;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        // // 确保两个NPC都面向对方
        // Vector3 direction1to2 = (positions.npc2Position - positions.npc1Position).normalized;
        // Vector3 direction2to1 = -direction1to2;
        
        // npc1.transform.rotation = Quaternion.LookRotation(direction1to2);
        // npc2.transform.rotation = Quaternion.LookRotation(direction2to1);
        
        if (npc1Arrived && npc2Arrived)
        {
            Debug.Log($"[NPCManager] 两个NPC已到达社交位置，准备就绪");
            // 触发事件广播准备就绪
            var eventArgs = new NPCEventArgs
            {
                npc = npc1,
                otherNPC = npc2,
                timestamp = System.DateTime.Now
            };
            GameEvents.TriggerNPCReadyForSocialInteraction(eventArgs);
        }
        else
        {
            Debug.LogWarning($"[NPCManager] 社交移动超时或失败, 取消该次社交互动");
        }
    }
}

/// <summary>
/// 社交位置数据
/// </summary>
public struct SocialPositions
{
    public Vector3 npc1Position;
    public Vector3 npc2Position;
    public Vector3 socialCenter;
    public Vector3 facingDirection;
}