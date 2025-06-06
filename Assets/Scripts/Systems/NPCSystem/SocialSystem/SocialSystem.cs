using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SocialSystem
{
    #region 配置参数
    [Header("配置文件")]
    [SerializeField] private SocialSystemConfig config;

    [Header("社交系统配置")]
    [SerializeField] private float interactionCheckInterval;  // 社交检查间隔
    [SerializeField] private float interactionRadius;         // 社交互动半径
    [SerializeField] private float interactionDuration;       // 社交互动持续时间
    [SerializeField] private int maxDailyInteractions;         // 每日最大互动次数
    
    [Header("好感度配置")]
    [SerializeField] private int baseRelationshipChange;       // 基础好感度变化
    [SerializeField] private int fightRelationshipPenalty;   // 争吵好感度惩罚
    [SerializeField] private int workTogetherBonus;            // 共同工作好感度奖励
    [SerializeField] private int relationshipDecayDaily;      // 每日好感度衰减
    #endregion

    #region 私有字段
    private List<NPC> npcs;
    private float lastCheckTime;
    private Dictionary<(NPC, NPC), float> interactionCooldowns;     // 互动冷却时间
    private Dictionary<NPC, int> dailyInteractionCounts;            // 每日互动计数
    private Dictionary<(NPC, NPC), SocialInteraction> activeInteractions; // 当前进行中的互动
    #endregion
    
    #region 初始化
    public void Initialize(List<NPC> npcList, SocialSystemConfig socialConfig = null) 
    { 
        npcs = npcList ?? new List<NPC>();
        interactionCooldowns = new Dictionary<(NPC, NPC), float>();
        dailyInteractionCounts = new Dictionary<NPC, int>();
        activeInteractions = new Dictionary<(NPC, NPC), SocialInteraction>();

        config = socialConfig ?? Resources.Load<SocialSystemConfig>("SocialSystemConfig");
    
        if (config == null)
        {
            Debug.LogError("[SocialSystem] SocialSystemConfig not found! Using default values.");
            // 可以创建一个默认配置或使用硬编码值
        }

        InitializeUsingConfig();

        // 订阅游戏事件
        GameEvents.OnDayChanged += OnDayChanged;
        
        Debug.Log($"[SocialSystem] 初始化完成，管理 {npcs.Count} 个NPC的社交关系");
    }

    public void InitializeUsingConfig()
    {
        interactionCheckInterval = config.interactionCheckInterval;
        interactionRadius = config.interactionRadius;
        interactionDuration = config.interactionDuration;
        maxDailyInteractions = config.maxDailyInteractions;
        baseRelationshipChange = config.baseRelationshipChange;
        fightRelationshipPenalty = config.fightRelationshipPenalty;
        workTogetherBonus = config.workTogetherBonus;
        relationshipDecayDaily = config.relationshipDecayDaily;
    }
    #endregion
    
    #region 主循环更新
    public void UpdateSocialInteractions()
    {
        if (Time.time - lastCheckTime < interactionCheckInterval) return;
        
        // 更新现有互动
        UpdateActiveInteractions();
        
        // 检查新的潜在互动
        CheckForPotentialInteractions();
        
        // 更新冷却时间
        UpdateInteractionCooldowns();
        
        lastCheckTime = Time.time;
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
                    StartInteraction(npc1, npc2);
                }
            }
        }
    }

    private bool CanInteract(NPC npc1, NPC npc2)
    {
        // 检查基本条件
        if (npc1 == npc2 || npc1.currentState != NPCState.Idle || npc2.currentState != NPCState.Idle)
            return false;
        
        // 检查距离
        float distance = Vector3.Distance(npc1.transform.position, npc2.transform.position);
        if (distance > interactionRadius)
            return false;
        
        // 检查冷却时间
        var key = GetInteractionKey(npc1, npc2);
        if (interactionCooldowns.ContainsKey(key) && interactionCooldowns[key] > 0)
            return false;
        
        // 检查每日互动次数
        if (GetDailyInteractionCount(npc1) >= maxDailyInteractions || 
            GetDailyInteractionCount(npc2) >= maxDailyInteractions)
            return false;
        
        // 检查是否已经在互动中
        if (IsInInteraction(npc1) || IsInInteraction(npc2))
            return false;
        
        return true;
    }

    private void StartInteraction(NPC npc1, NPC npc2)
    {
        var interaction = new SocialInteraction(npc1, npc2, interactionDuration);
        var key = GetInteractionKey(npc1, npc2);
        
        activeInteractions[key] = interaction;
        
        // 设置NPC状态为社交
        npc1.ChangeState(NPCState.Social);
        npc2.ChangeState(NPCState.Social);
        
        // 增加每日互动计数
        IncrementDailyInteractionCount(npc1);
        IncrementDailyInteractionCount(npc2);
        
        Debug.Log($"[SocialSystem] {npc1.data.npcName} 和 {npc2.data.npcName} 开始社交互动");
        
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

    private void CompleteInteraction(NPC npc1, NPC npc2, SocialInteraction interaction)
    {
        // 计算互动结果
        bool isFight = WillNPCsFight(npc1, npc2);
        int relationshipChange;
        
        if (isFight)
        {
            relationshipChange = ProcessFight(npc1, npc2);
            Debug.Log($"[SocialSystem] {npc1.data.npcName} 和 {npc2.data.npcName} 发生了争吵！");
        }
        else
        {
            relationshipChange = ProcessFriendlyChat(npc1, npc2);
            Debug.Log($"[SocialSystem] {npc1.data.npcName} 和 {npc2.data.npcName} 愉快地聊天了");
        }
        
        // 设置冷却时间
        var key = GetInteractionKey(npc1, npc2);
        float cooldownTime = isFight ? interactionCheckInterval * 3 : interactionCheckInterval;
        interactionCooldowns[key] = cooldownTime;
        
        // 恢复NPC状态
        npc1.ChangeState(NPCState.Idle);
        npc2.ChangeState(NPCState.Idle);
        
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
        
        Debug.Log("[SocialSystem] 处理每日好感度衰减完成");
    }
    #endregion

    #region 辅助方法
    // 辅助方法
    private (NPC, NPC) GetInteractionKey(NPC npc1, NPC npc2)
    {
        // 确保键的一致性，无论传入顺序如何
        return npc1.GetInstanceID() < npc2.GetInstanceID() ? (npc1, npc2) : (npc2, npc1);
    }
    
    private void UpdateInteractionCooldowns()
    {
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
#endregion