using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.AI;

public class SocialSystem
{
    #region 配置参数
    [Header("配置文件")]
    [SerializeField] private SocialSystemConfig config;

    [Header("社交系统配置")]
    [SerializeField] private float interactionCheckInterval;  // 社交检查间隔
    [SerializeField] private float interactionRadius;         // 社交互动检测半径
    [SerializeField] private float interactionDuration;       // 社交互动持续时间
    [SerializeField] public float interactionCooldown{get; private set;}       // 社交互动冷却时间
    [SerializeField] public float personalSocialCooldown{get; private set;}    // 个人社交冷却时间
    [SerializeField] private int maxDailyInteractions;         // 每日最大互动次数
    [SerializeField] public float socialInteractionDistance{get; private set;} = 2f;     // 社交距离
    [SerializeField] public float socialMoveSpeed{get; private set;} = 0.5f;               // 社交移动速度
    [SerializeField] public float socialTimeout{get; private set;} = 10f;                // 社交移动超时时间
    
    [Header("好感度配置")]
    [SerializeField] private int baseRelationshipChange;       // 基础好感度变化
    [SerializeField] private int fightRelationshipPenalty;   // 争吵好感度惩罚
    [SerializeField] private int workTogetherBonus;            // 共同工作好感度奖励
    [SerializeField] private int relationshipDecayDaily;      // 每日好感度衰减

    [Header("邀请系统配置")]
    [SerializeField] private float invitationTimeout = 5f;
    [SerializeField] private int maxPendingInvitations = 3;     // 最大待处理邀请数

    // 邀请管理
    private Dictionary<int, SocialInvitation> activeInvitations = new Dictionary<int, SocialInvitation>();
    private Queue<SocialInvitationResponse> pendingResponses = new Queue<SocialInvitationResponse>();
    private int nextInvitationId = 1;
    #endregion

    #region 私有字段
    private List<NPC> npcs;
    private float lastCheckTime;
    public Dictionary<(NPC, NPC), float> interactionCooldowns{get; private set;}     // 互动冷却时间
    public Dictionary<NPC, float> personalSocialCooldowns{get; private set;}    // 个人社交冷却时间
    public Dictionary<NPC, int> dailyInteractionCounts{get; private set;}            // 每日互动计数
    public Dictionary<(NPC, NPC), SocialInteraction> activeInteractions{get; private set;} // 当前进行中的互动
    
    // 新增：协程管理
    private Dictionary<(NPC, NPC), Coroutine> activeSocialCoroutines = new Dictionary<(NPC, NPC), Coroutine>();
    #endregion
    
    #region 初始化
    public void Initialize(List<NPC> npcList, SocialSystemConfig socialConfig = null) 
    { 
        npcs = npcList ?? new List<NPC>();
        interactionCooldowns = new Dictionary<(NPC, NPC), float>();
        personalSocialCooldowns = new Dictionary<NPC, float>();
        dailyInteractionCounts = new Dictionary<NPC, int>();
        activeInteractions = new Dictionary<(NPC, NPC), SocialInteraction>();
        activeSocialCoroutines = new Dictionary<(NPC, NPC), Coroutine>();

        config = socialConfig ?? Resources.Load<SocialSystemConfig>("SocialSystemConfig");
    
        if (config == null)
        {
            Debug.LogError("[SocialSystem] SocialSystemConfig not found! Using default values.");
            // 可以创建一个默认配置或使用硬编码值
        }

        InitializeUsingConfig();

        // 订阅游戏事件
        GameEvents.OnDayChanged += OnDayChanged;
        
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialSystem] 初始化完成，管理 {npcs.Count} 个NPC的社交关系");
    }

    public void InitializeUsingConfig()
    {
        interactionCheckInterval = config.interactionCheckInterval;
        interactionRadius = config.interactionRadius;
        interactionDuration = config.interactionDuration;
        interactionCooldown = config.interactionCooldown;
        personalSocialCooldown = config.personalSocialCooldown;
        maxDailyInteractions = config.maxDailyInteractions;
        baseRelationshipChange = config.baseRelationshipChange;
        fightRelationshipPenalty = config.fightRelationshipPenalty;
        workTogetherBonus = config.workTogetherBonus;
        relationshipDecayDaily = config.relationshipDecayDaily;
        socialInteractionDistance = config.socialInteractionDistance;
        socialMoveSpeed = config.socialMoveSpeed;
        socialTimeout = config.socialTimeout;
    }
    #endregion
    
    #region 主循环更新
    public void UpdateSocialInteractions()
    {
        // 更新现有互动 - 每帧都要更新，不受时间间隔限制
        UpdateActiveInteractions();
                    
        // 更新冷却时间
        UpdateInteractionCooldowns();

        // 更新邀请系统
        UpdateInvitationSystem();
        
        // // 检查新的潜在互动和其他操作 - 受时间间隔限制
        // if (Time.time - lastCheckTime >= interactionCheckInterval)
        // {
        //     if(NPCManager.Instance.showDebugInfo) 
        //         Debug.Log($"[SocialSystem] 社交系统定期更新");
            
        //     // 检查新的潜在互动
        //     CheckForPotentialInteractions();

        //     lastCheckTime = Time.time;
        // }
    }
    
    private void UpdateActiveInteractions()
    {
        var completedInteractions = new List<(NPC, NPC)>();
        
        if (activeInteractions == null) return;
        foreach (var kvp in activeInteractions)
        {
            var interaction = kvp.Value;
            
            if (interaction.Update())
            {
                // 互动完成
                CompleteInteraction(kvp.Key.Item1, kvp.Key.Item2, interaction);
                completedInteractions.Add(kvp.Key);
            }
        }
        
        // 移除完成的互动
        foreach (var key in completedInteractions)
        {
            activeInteractions.Remove(key);
        }
    }
    #endregion

    #region 互动管理
    private void CheckForPotentialInteractions() 
    {
        if (npcs == null || npcs.Count == 0) return;
        // 遍历所有空闲NPC并尝试互动
        var idleNPCs = npcs.Where(npc => npc.currentState == NPCState.Idle).ToList();
        
        for (int i = 0; i < idleNPCs.Count; i++)
        {
            for (int j = i + 1; j < idleNPCs.Count; j++)
            {
                var npc1 = idleNPCs[i];
                var npc2 = idleNPCs[j];
                
                if (CanInteract(npc1, npc2))
                {
                    PrepareForSocialInteraction(npc1, npc2);
                }
            }
        }
    }

    private bool CanInteract(NPC npc1, NPC npc2)
    {
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialSystem] 检查两个NPC是否可以互动: {npc1.data.npcName} 和 {npc2.data.npcName}");
        // 检查基本条件，仅有Idle状态的NPC才能进行社交互动
        if (npc1 == npc2 || 
        (npc1.currentState != NPCState.Idle && npc1.currentState != NPCState.PrepareForSocial) || 
        (npc2.currentState != NPCState.Idle && npc2.currentState != NPCState.PrepareForSocial))
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC不能互动: {npc1.data.npcName} 和 {npc2.data.npcName}");
            return false;
        }
        
        // 检查NavMesh中的实际距离
        float navMeshDistance = CalculateNavMeshDistance(npc1.transform.position, npc2.transform.position);
        if (navMeshDistance > interactionRadius || navMeshDistance < 0) // navMeshDistance < 0 表示无法到达
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC距离太远: {npc1.data.npcName} 和 {npc2.data.npcName}");
            return false;
        }
        
        // 检查NPC对的交互冷却时间
        var key = GetInteractionKey(npc1, npc2);
        if (interactionCooldowns.ContainsKey(key) && interactionCooldowns[key] > 0)
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC冷却时间未到: {npc1.data.npcName} 和 {npc2.data.npcName}");
            return false;
        }
        
        // 检查NPC的个人社交冷却时间
        if ((personalSocialCooldowns.ContainsKey(npc1) && personalSocialCooldowns[npc1] > 0) ||
            (personalSocialCooldowns.ContainsKey(npc2) && personalSocialCooldowns[npc2] > 0))
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC个人社交冷却时间未到: {npc1.data.npcName} 和 {npc2.data.npcName}");
            return false;
        }
        
        // 检查每日互动次数
        if (GetDailyInteractionCount(npc1) >= maxDailyInteractions || 
            GetDailyInteractionCount(npc2) >= maxDailyInteractions)
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC每日互动次数已满: {npc1.data.npcName} 和 {npc2.data.npcName}");
            return false;
        }
        
        // 检查是否已经在互动中
        if (IsInInteraction(npc1) || IsInInteraction(npc2))
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC已经在互动中: {npc1.data.npcName} 和 {npc2.data.npcName}");
            return false;
        }
        
        // 检查是否已经有正在进行的社交协程
        var socialKey = GetInteractionKey(npc1, npc2);
        if (activeSocialCoroutines.ContainsKey(socialKey))
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC已经有正在进行的社交协程: {npc1.data.npcName} 和 {npc2.data.npcName}");
            return false;
        }

        // 检测两个NPC是否并未处在各自的休息时间段
        if(npc1.IsRestTime() || npc2.IsRestTime())
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC处在各自的休息时间段: {npc1.data.npcName} 和 {npc2.data.npcName}");
            return false;
        }
        
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialSystem] 两个NPC可以互动: {npc1.data.npcName} 和 {npc2.data.npcName}");
        return true;
    }
    
    private void PrepareForSocialInteraction(NPC npc1, NPC npc2)
    {
        // 开启社交协程
        var socialKey = GetInteractionKey(npc1, npc2);
        var coroutine = NPCManager.Instance.StartCoroutine(ExecuteSocialInteractionSequence(npc1, npc2));
        activeSocialCoroutines[socialKey] = coroutine;
    }

    /// <summary>
    /// 执行完整的社交互动序列
    /// </summary>
    private IEnumerator ExecuteSocialInteractionSequence(NPC npc1, NPC npc2)
    {
        var socialKey = GetInteractionKey(npc1, npc2);
        
        try
        {
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 开始社交互动序列: {npc1.data.npcName} 和 {npc2.data.npcName}");

            // 阶段1: 广播社交事件，让NPC进入PrepareForSocial状态
            var eventArgs = new NPCEventArgs
            {
                npc = npc1,
                otherNPC = npc2,
                timestamp = System.DateTime.Now
            };
            GameEvents.TriggerNPCShouldStartSocialInteraction(eventArgs);

            // 阶段2: 等待两个NPC都进入MovingToSocial状态
            float waitStartTime = Time.time;
            while ((npc1.currentState != NPCState.MovingToSocial || npc2.currentState != NPCState.MovingToSocial) 
                   && (Time.time - waitStartTime) < socialTimeout)
            {
                yield return new WaitForSeconds(0.3f);
            }

            // 检查是否超时或NPC状态异常
            if (npc1.currentState != NPCState.MovingToSocial || npc2.currentState != NPCState.MovingToSocial)
            {
                Debug.LogWarning($"[SocialSystem] NPC未能进入MovingToSocial状态，取消社交互动: {npc1.data.npcName}, {npc2.data.npcName}");
                npc1.ChangeState(NPCState.Idle);
                npc2.ChangeState(NPCState.Idle);
                yield break;
            }

            // 阶段3: 计算社交位置并让NPC移动
            var socialPositions = CalculateSocialPositions(npc1, npc2);
            
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 计算社交位置: {socialPositions.npc1Position} 和 {socialPositions.npc2Position}");

            // 让两个NPC移动到社交位置
            npc1.MoveToTarget(socialPositions.npc1Position);
            npc2.MoveToTarget(socialPositions.npc2Position);

            // 阶段4: 等待两个NPC到达社交位置
            waitStartTime = Time.time;
            while ((!npc1.isInPosition || !npc2.isInPosition) 
                   && (Time.time - waitStartTime) < socialTimeout)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // 检查是否成功到达
            if (!npc1.isInPosition || !npc2.isInPosition)
            {
                Debug.LogWarning($"[SocialSystem] NPC未能到达社交位置，取消社交互动: {npc1.data.npcName}, {npc2.data.npcName}");
                npc1.ChangeState(NPCState.Idle);
                npc2.ChangeState(NPCState.Idle);
                yield break;
            }

            // 阶段5: 让NPC面向对方
            npc1.TurnToPosition(npc2.transform.position);
            npc2.TurnToPosition(npc1.transform.position);

            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] 两个NPC已就位，开始社交互动: {npc1.data.npcName}, {npc2.data.npcName}");

            // 阶段6: 开始社交互动
            StartInteraction(npc1, npc2);
        }
        finally
        {
            // 清理协程记录
            if (activeSocialCoroutines.ContainsKey(socialKey))
            {
                activeSocialCoroutines.Remove(socialKey);
            }
        }
    }

    private void StartInteraction(NPC npc1, NPC npc2)
    {
        if (npc1 == null || npc2 == null) {
            Debug.LogError("[SocialSystem] NPC为空，无法开始社交互动");
            return;
        }
        
        var interaction = new SocialInteraction(npc1, npc2, interactionDuration);
        var key = GetInteractionKey(npc1, npc2);
        
        activeInteractions[key] = interaction;
        
        // 设置NPC状态为社交
        npc1.ChangeState(NPCState.Social);
        npc2.ChangeState(NPCState.Social);
        
        // 增加每日互动计数
        IncrementDailyInteractionCount(npc1);
        IncrementDailyInteractionCount(npc2);
        
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialSystem] {npc1.data.npcName} 和 {npc2.data.npcName} 开始社交互动");
        DebugDrawSocialInteraction(npc1, npc2);
        
        // 触发事件
        var eventArgs = new NPCEventArgs
        {
            npc = npc1,
            eventType = NPCEventArgs.NPCEventType.SocialInteraction,
            otherNPC = npc2,
            timestamp = System.DateTime.Now
        };
        GameEvents.TriggerNPCSocialInteractionStarted(eventArgs);
    }

    /// <summary>
    /// 计算两个NPC的社交位置
    /// </summary>
    public SocialPositions CalculateSocialPositions(NPC npc1, NPC npc2)
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
        float halfDistance = socialInteractionDistance * 0.5f;
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

    private void CompleteInteraction(NPC npc1, NPC npc2, SocialInteraction interaction)
    {
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialSystem] {npc1.data.npcName} 和 {npc2.data.npcName} 社交互动结束");
        // 计算互动结果
        bool isFight = WillNPCsFight(npc1, npc2);
        int relationshipChange;
        
        if (isFight)
        {
            relationshipChange = ProcessFight(npc1, npc2);
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] {npc1.data.npcName} 和 {npc2.data.npcName} 发生了争吵！互相之间好感度下降了 {relationshipChange}");
            npc1.ChangeState(NPCState.SocialEndFight);
            npc2.ChangeState(NPCState.SocialEndFight);
        }
        else
        {
            relationshipChange = ProcessFriendlyChat(npc1, npc2);
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialSystem] {npc1.data.npcName} 和 {npc2.data.npcName} 愉快地聊天了！互相之间好感度上升了 {relationshipChange}");
            npc1.ChangeState(NPCState.SocialEndHappy);
            npc2.ChangeState(NPCState.SocialEndHappy);
        }
        
        // 设置冷却时间
        var key = GetInteractionKey(npc1, npc2);
        interactionCooldowns[key] = interactionCooldown;
        personalSocialCooldowns[npc1] = personalSocialCooldown;
        personalSocialCooldowns[npc2] = personalSocialCooldown;
        
        // 触发关系变化事件
        var eventArgs = new NPCEventArgs
        {
            npc = npc1,
            eventType = NPCEventArgs.NPCEventType.RelationshipChanged,
            otherNPC = npc2,
            relationshipChange = relationshipChange,
            timestamp = System.DateTime.Now
        };
        GameEvents.TriggerNPCRelationshipChanged(eventArgs);
        GameEvents.TriggerNPCSocialInteractionEnded(eventArgs);
    }
    #endregion

    #region 冲突和争吵逻辑
    private bool WillNPCsFight(NPC npc1, NPC npc2)
    {
        // 基础争吵概率
        float baseFightChance = 0.2f;
        
        // 性格冲突检查
        float personalityConflict = GetPersonalityConflictModifier(npc1, npc2);
        
        // 好感度影响
        float relationshipModifier = GetRelationshipModifier(npc1, npc2);
        
        // 特殊词条影响
        float traitModifier = GetTraitModifier(npc1, npc2);
        
        float finalFightChance = baseFightChance + personalityConflict - relationshipModifier + traitModifier;
        finalFightChance = Mathf.Clamp01(finalFightChance);
        
        return Random.Range(0f, 1f) < finalFightChance;
    }

    private float GetPersonalityConflictModifier(NPC npc1, NPC npc2)
    {
        var personality1 = npc1.data.personality;
        var personality2 = npc2.data.personality;
        
        // 相反性格增加争吵概率
        Dictionary<NPCPersonalityType, NPCPersonalityType> opposites = new Dictionary<NPCPersonalityType, NPCPersonalityType>
        {
            { NPCPersonalityType.Diligent, NPCPersonalityType.Lazy },
            { NPCPersonalityType.Lazy, NPCPersonalityType.Diligent },
            { NPCPersonalityType.Honest, NPCPersonalityType.Hypocritical },
            { NPCPersonalityType.Hypocritical, NPCPersonalityType.Honest },
            { NPCPersonalityType.Kind, NPCPersonalityType.Evil },
            { NPCPersonalityType.Evil, NPCPersonalityType.Kind }
        };
        
        if (opposites.ContainsKey(personality1) && opposites[personality1] == personality2)
        {
            return 0.3f; // 相反性格增加30%争吵概率
        }
        
        return 0f;
    }

    private float GetRelationshipModifier(NPC npc1, NPC npc2)
    {
        int relationship = npc1.GetRelationshipWith(npc2);
        
        // 好感度越高，争吵概率越低
        if (relationship >= 80) return 0.4f;
        if (relationship >= 60) return 0.2f;
        if (relationship >= 40) return 0.1f;
        if (relationship <= 20) return -0.2f; // 低好感度增加争吵概率
        
        return 0f;
    }
    
    private float GetTraitModifier(NPC npc1, NPC npc2)
    {
        float modifier = 0f;
        
        // 交友大师减少争吵概率
        if (npc1.HasTrait(NPCTraitType.SocialMaster) || npc2.HasTrait(NPCTraitType.SocialMaster))
        {
            modifier -= 0.15f;
        }
        
        // 舔狗减少争吵概率
        if (npc1.HasTrait(NPCTraitType.Bootlicker) || npc2.HasTrait(NPCTraitType.Bootlicker))
        {
            modifier -= 0.1f;
        }
        
        return modifier;
    }
    #endregion

    #region 好感度处理
    private int ProcessFight(NPC npc1, NPC npc2)
    {
        int penalty = fightRelationshipPenalty;
        
        // 应用好感度变化
        npc1.DecreaseRelationship(npc2, penalty);
        npc2.DecreaseRelationship(npc1, penalty);
        
        return penalty;
    }

    private int ProcessFriendlyChat(NPC npc1, NPC npc2)
    {
        int bonus = baseRelationshipChange;
        
        // 特殊词条加成
        if (npc1.HasTrait(NPCTraitType.SocialMaster))
        {
            bonus = Mathf.RoundToInt(bonus * 1.5f);
        }
        if (npc2.HasTrait(NPCTraitType.SocialMaster))
        {
            bonus = Mathf.RoundToInt(bonus * 1.5f);
        }
        
        if (npc1.HasTrait(NPCTraitType.Bootlicker))
        {
            bonus = Mathf.RoundToInt(bonus * 1.2f);
        }
        if (npc2.HasTrait(NPCTraitType.Bootlicker))
        {
            bonus = Mathf.RoundToInt(bonus * 1.2f);
        }
        
        // 应用好感度变化
        npc1.IncreaseRelationship(npc2, bonus);
        npc2.IncreaseRelationship(npc1, bonus);
        
        return bonus;
    }
    
    // 共同工作的好感度奖励
    public void ProcessWorkTogetherBonus(NPC npc1, NPC npc2)
    {
        if (npc1 == null || npc2 == null || npc1 == npc2) return;
        
        int bonus = workTogetherBonus;
        
        // 特殊词条影响
        if (npc1.HasTrait(NPCTraitType.SocialMaster) || npc2.HasTrait(NPCTraitType.SocialMaster))
        {
            bonus = Mathf.RoundToInt(bonus * 1.3f);
        }
        
        npc1.IncreaseRelationship(npc2, bonus);
        npc2.IncreaseRelationship(npc1, bonus);
        
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialSystem] {npc1.data.npcName} 和 {npc2.data.npcName} 因共同工作获得好感度 +{bonus}");
    }
    #endregion

    #region 每日事件处理
    private void OnDayChanged(TimeEventArgs args)
    {
        // 重置每日互动计数
        dailyInteractionCounts.Clear();
        
        // 处理好感度衰减
        ProcessDailyRelationshipDecay();
    }

    private void ProcessDailyRelationshipDecay()
    {
        foreach (var npc in npcs)
        {
            if (npc.relationships == null) continue;
            
            var relationshipKeys = npc.relationships.Keys.ToList();
            foreach (var otherNPC in relationshipKeys)
            {
                if (otherNPC == null) continue;
                
                int currentRelationship = npc.relationships[otherNPC];
                
                // 只有好感度大于50的才会衰减，避免负面关系无限恶化
                if (currentRelationship > 50)
                {
                    npc.DecreaseRelationship(otherNPC, -relationshipDecayDaily);
                }
            }
        }
        
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log("[SocialSystem] 处理每日好感度衰减完成");
    }
    #endregion
    
    #region 邀请发送和接收
    /// <summary>
    /// 发送社交邀请
    /// </summary>
    public bool SendSocialInvitation(NPC sender, NPC receiver)
    {
        // 1. 验证发送条件
        if (!CanSendInvitation(sender, receiver))
        {
            return false;
        }
        
        // 2. 创建邀请
        var invitation = new SocialInvitation
        {
            invitationId = nextInvitationId++,
            sender = sender,
            receiver = receiver,
            suggestedSocialLocaiton = CalculateSocialPositions(sender, receiver),
            sendTime = Time.time,
            expireTime = Time.time + invitationTimeout,
            status = SocialInvitationStatus.Pending
        };
        
        // 3. 注册邀请
        activeInvitations[invitation.invitationId] = invitation;
        
        // 4. 通知接收者（通过状态机）
        NotifyInvitationReceived(receiver, invitation);
        
        if (NPCManager.Instance.showDebugInfo)
            Debug.Log($"[SocialSystem] {sender.data.npcName} 向 {receiver.data.npcName} 发送社交邀请 (ID: {invitation.invitationId})");
        
        return true;
    }

    /// <summary>
    /// 响应社交邀请
    /// </summary>
    public void RespondToInvitation(int invitationId, NPC responder, bool accepted, string reason = "")
    {
        if (!activeInvitations.TryGetValue(invitationId, out var invitation))
        {
            Debug.LogWarning($"[SocialSystem] 邀请 {invitationId} 不存在或已处理");
            return;
        }
        
        if (invitation.receiver != responder)
        {
            Debug.LogError($"[SocialSystem] 响应者 {responder.data.npcName} 不是邀请 {invitationId} 的接收者");
            return;
        }
        
        if (!invitation.IsValid)
        {
            Debug.LogWarning($"[SocialSystem] 邀请 {invitationId} 已过期或无效");
            return;
        }
        
        // 更新邀请状态
        invitation.status = accepted ? SocialInvitationStatus.Accepted : SocialInvitationStatus.Declined;
        
        // 创建响应
        var response = new SocialInvitationResponse
        {
            invitationId = invitationId,
            responder = responder,
            accepted = accepted,
            reason = reason
        };
        
        // 加入待处理响应队列
        pendingResponses.Enqueue(response);
        
        if (NPCManager.Instance.showDebugInfo)
            Debug.Log($"[SocialSystem] {responder.data.npcName} {(accepted ? "接受" : "拒绝")}了邀请 {invitationId} {reason}");
    }
    
    #endregion

    #region 状态机查询接口
    
    /// <summary>
    /// 获取NPC的待处理邀请
    /// </summary>
    public SocialInvitation GetPendingInvitation(NPC npc)
    {
        return activeInvitations.Values
            .Where(inv => inv.receiver == npc && inv.IsValid)
            .OrderBy(inv => inv.sendTime)
            .FirstOrDefault();
    }
    
    /// <summary>
    /// 获取并消费邀请响应（给发送者状态机使用）
    /// </summary>
    public SocialInvitationResponse GetInvitationResponse(NPC sender)
    {
        // 查找属于该发送者的响应
        var responses = pendingResponses.ToArray();
        pendingResponses.Clear();
        
        foreach (var response in responses)
        {
            if (activeInvitations.TryGetValue(response.invitationId, out var invitation) && 
                invitation.sender == sender)
            {
                // 找到属于该发送者的响应，其他响应放回队列
                foreach (var otherResponse in responses)
                {
                    if (otherResponse != response)
                        pendingResponses.Enqueue(otherResponse);
                }
                return response;
            }
        }
        
        // 没找到，所有响应放回队列
        foreach (var response in responses)
        {
            pendingResponses.Enqueue(response);
        }
        
        return null;
    }
    
    /// <summary>
    /// 检查是否可以发送邀请
    /// </summary>
    private bool CanSendInvitation(NPC sender, NPC receiver)
    {
        // 1. 基本验证
        if (sender == null || receiver == null || sender == receiver)
            return false;
        
        // 2. 检查接收者状态
        if (receiver.currentState != NPCState.Idle)
        {
            if (NPCManager.Instance.showDebugInfo)
                Debug.Log($"[SocialSystem] {receiver.data.npcName} 不在空闲状态，无法接收邀请");
            return false;
        }
        
        // 3. 检查是否已有待处理邀请
        bool hasExistingInvitation = activeInvitations.Values.Any(inv => 
            (inv.sender == sender && inv.receiver == receiver || 
             inv.sender == receiver && inv.receiver == sender) && 
            inv.IsValid);
        
        if (hasExistingInvitation)
        {
            if (NPCManager.Instance.showDebugInfo)
                Debug.Log($"[SocialSystem] {sender.data.npcName} 和 {receiver.data.npcName} 之间已有待处理邀请");
            return false;
        }
        
        // 4. 检查社交条件（距离、冷却时间等）
        if (!CanInteract(sender, receiver))
        {
            return false;
        }
        
        // 5. 检查接收者待处理邀请数量
        int receiverPendingCount = activeInvitations.Values.Count(inv => 
            inv.receiver == receiver && inv.IsValid);
        
        if (receiverPendingCount >= maxPendingInvitations)
        {
            if (NPCManager.Instance.showDebugInfo)
                Debug.Log($"[SocialSystem] {receiver.data.npcName} 待处理邀请已满");
            return false;
        }
        
        return true;
    }
    
    #endregion
    #region 辅助方法
    // 计算NavMesh中的实际距离
    private float CalculateNavMeshDistance(Vector3 startPos, Vector3 endPos)
    {
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        
        // 尝试计算路径
        if (UnityEngine.AI.NavMesh.CalculatePath(startPos, endPos, UnityEngine.AI.NavMesh.AllAreas, path))
        {
            // 如果路径状态不完整，返回-1表示无法到达
            if (path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                return -1f;
            }
            
            // 计算路径总长度
            float distance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            
            return distance;
        }
        
        // 如果无法计算路径，返回-1
        return -1f;
    }
    
    // 辅助方法
    private (NPC, NPC) GetInteractionKey(NPC npc1, NPC npc2)
    {
        // 确保键的一致性，无论传入顺序如何
        return npc1.GetInstanceID() < npc2.GetInstanceID() ? (npc1, npc2) : (npc2, npc1);
    }
    
    private void UpdateInteractionCooldowns()
    {
        // 更新NPC对的交互冷却时间
        if (interactionCooldowns == null) return;
        var keys = interactionCooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            interactionCooldowns[key] -= Time.deltaTime;
            if (interactionCooldowns[key] <= 0)
            {
                interactionCooldowns.Remove(key);
            }
        }
        // 更新个人社交冷却时间
        if (personalSocialCooldowns == null) return;
        var personalKeys = personalSocialCooldowns.Keys.ToList();
        foreach (var key in personalKeys)
        {
            personalSocialCooldowns[key] -= Time.deltaTime;
            if (personalSocialCooldowns[key] <= 0)
            {
                personalSocialCooldowns.Remove(key);
            }
        }
    }
    
    private int GetDailyInteractionCount(NPC npc)
    {
        return dailyInteractionCounts.GetValueOrDefault(npc, 0);
    }

    private void IncrementDailyInteractionCount(NPC npc)
    {
        if (!dailyInteractionCounts.ContainsKey(npc))
        {
            dailyInteractionCounts[npc] = 0;
        }
        dailyInteractionCounts[npc]++;
    }

    private bool IsInInteraction(NPC npc)
    {
        return activeInteractions.Any(kvp => kvp.Key.Item1 == npc || kvp.Key.Item2 == npc);
    }
    /// <summary>
    /// 为指定NPC找到潜在的社交伙伴
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public List<NPC> FindPotentialSocialPartners(NPC npc){
        List<NPC> potentialPartners = new List<NPC>();
        foreach (var otherNPC in npcs)
        {
            if (otherNPC == npc) continue;
            if (CanInteract(npc, otherNPC))
            {
                potentialPartners.Add(otherNPC);
            }
        }
        return potentialPartners;
    }

    /// <summary>
    /// 为指定NPC找到最近的潜在社交伙伴
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public NPC FindNearestSocialPartner(NPC npc){
        List<NPC> potentialPartners = FindPotentialSocialPartners(npc);
        if (potentialPartners.Count == 0) return null;
        return potentialPartners.OrderBy(p => Vector3.Distance(npc.transform.position, p.transform.position)).First();
    }
    
    /// <summary>
    /// 通知NPC收到邀请（通过状态机的Update检查）
    /// </summary>
    private void NotifyInvitationReceived(NPC receiver, SocialInvitation invitation)
    {
        // 不直接修改NPC状态，让状态机在Update中主动检查
        // 这样保持了状态机的主导地位
    }
    
    /// <summary>
    /// 清理过期邀请
    /// </summary>
    private void CleanupExpiredInvitations()
    {
        var expiredIds = activeInvitations.Values
            .Where(inv => inv.IsExpired)
            .Select(inv => inv.invitationId)
            .ToList();
        
        foreach (var id in expiredIds)
        {
            activeInvitations.Remove(id);
        }
    }
    

    public void UpdateInvitationSystem()
    {
        CleanupExpiredInvitations();
        
        // 这里不再主动扫描和发送邀请
        // 所有邀请都由NPC状态机主动发起
    }
    #endregion

    #region 统计和调试
    public SocialSystemStats GetStats()
    {
        return new SocialSystemStats
        {
            totalNPCs = npcs.Count,
            activeInteractions = activeInteractions.Count,
            averageRelationship = CalculateAverageRelationship(),
            dailyInteractionsTotal = dailyInteractionCounts.Values.Sum()
        };
    }
    
    private float CalculateAverageRelationship()
    {
        float totalRelationship = 0f;
        int relationshipCount = 0;
        
        foreach (var npc in npcs)
        {
            if (npc.relationships == null) continue;
            
            foreach (var relationship in npc.relationships.Values)
            {
                totalRelationship += relationship;
                relationshipCount++;
            }
        }
        
        return relationshipCount > 0 ? totalRelationship / relationshipCount : 50f;
    }

    public void DebugDrawSocialInteraction(NPC npc1, NPC npc2)
    {
        Debug.DrawLine(npc1.transform.position, npc2.transform.position, Color.red, 10f);
    }

    #endregion
}

#region 社交互动类
// 社交互动类
public class SocialInteraction
{
    public NPC npc1 { get; private set; }
    public NPC npc2 { get; private set; }
    public float duration { get; private set; }
    public float elapsed { get; private set; }
    public bool isCompleted { get; private set; }
    
    public SocialInteraction(NPC npc1, NPC npc2, float duration)
    {
        this.npc1 = npc1;
        this.npc2 = npc2;
        this.duration = duration;
        this.elapsed = 0f;
        this.isCompleted = false;
    }
    
    public bool Update()
    {
        if (isCompleted) return true;
        
        elapsed += Time.deltaTime;
        
        if (elapsed >= duration)
        {
            isCompleted = true;
            return true;
        }
        
        return false;
    }
}


#endregion

#region 数据结构
// 社交系统统计信息
[System.Serializable]
public class SocialSystemStats
{
    public int totalNPCs;
    public int activeInteractions;
    public float averageRelationship;
    public int dailyInteractionsTotal;
}

#region 社交邀请
public class SocialInvitation{
    public int invitationId;
    public NPC sender;
    public NPC receiver;
    public SocialPositions suggestedSocialLocaiton;
    public float sendTime;
    public float expireTime;
    public SocialInvitationStatus status;

    public bool IsExpired => Time.time > expireTime;
    public bool IsValid => status == SocialInvitationStatus.Pending && !IsExpired;

}

public enum SocialInvitationStatus
{
    Pending,    // 等待响应
    Accepted,   // 已接受
    Declined,   // 已拒绝
    Expired     // 已过期
}

public class SocialInvitationResponse
{
    public int invitationId;
    public NPC responder;
    public bool accepted;
    public string reason; // 拒绝原因（调试用）
}
public enum InvitationPriority
{
    Low = 0,        // 一般邀请
    Normal = 1,     // 正常邀请
    High = 2        // 高优先级邀请（如关系很好的NPC发出的）
}
#endregion
#endregion

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