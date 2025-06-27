using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 社交关系管理器 - 管理所有好感度和关系相关逻辑
/// </summary>
public class SocialRelationshipManager
{
    private SocialSystemData data;
    // 默认关系值
    public const int DEFAULT_RELATIONSHIP = 50;
    
    public SocialRelationshipManager(SocialSystemData systemData)
    {
        data = systemData;
    }

    #region 关系数据管理
    /// <summary>
    /// 获取NPC之间的关系
    /// </summary>
    public int GetRelationship(NPC npc1, NPC npc2)
    {
        if (npc1 == null || npc2 == null || npc1 == npc2)
            return DEFAULT_RELATIONSHIP;
        
        var key = (npc1.NpcId, npc2.NpcId);
        return data.relationships.TryGetValue(key, out var value) ? value : DEFAULT_RELATIONSHIP;
    }
    /// <summary>
    /// 设置两个NPC之间的关系值（单向）
    /// </summary>
    public void SetRelationship(NPC npc1, NPC npc2, int value)
    {
        if (npc1 == null || npc2 == null || npc1 == npc2) 
            return;
            
        var key = (npc1.NpcId, npc2.NpcId);
        value = Mathf.Clamp(value, data.config.minRelationship, data.config.maxRelationship);
        data.relationships[key] = value;
        
        // 触发关系变化事件
        var eventArgs = new NPCEventArgs
        {
            npc = npc1,
            otherNPC = npc2,
            relationshipChange = value - GetRelationship(npc1, npc2),
            eventType = NPCEventArgs.NPCEventType.RelationshipChanged
        };
        GameEvents.TriggerNPCRelationshipChanged(eventArgs);
    }
    /// <summary>
    /// 增加关系值
    /// </summary>
    public void IncreaseRelationship(NPC npc1, NPC npc2, int amount)
    {
        int currentValue = GetRelationship(npc1, npc2);
        SetRelationship(npc1, npc2, currentValue + amount);
    }
    /// <summary>
    /// 减少关系值
    /// </summary>
    public void DecreaseRelationship(NPC npc1, NPC npc2, int amount)
    {
        int currentValue = GetRelationship(npc1, npc2);
        SetRelationship(npc1, npc2, currentValue - amount);
    }    
    /// <summary>
    /// 获取某个NPC对所有其他NPC的关系
    /// </summary>
    public Dictionary<NPC, int> GetAllRelationshipsFor(NPC npc)
    {
        var result = new Dictionary<NPC, int>();
        
        if (npc == null) return result;
        
        foreach (var otherNpc in data.npcs)
        {
            if (otherNpc != npc)
            {
                result[otherNpc] = GetRelationship(npc, otherNpc);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 移除某个NPC的所有关系数据
    /// </summary>
    public void RemoveAllRelationshipsFor(NPC npc)
    {
        if (npc == null) return;
        
        var keysToRemove = new List<(string, string)>();
        
        foreach (var key in data.relationships.Keys)
        {
            if (key.Item1 == npc.NpcId || key.Item2 == npc.NpcId)
            {
                keysToRemove.Add(key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            data.relationships.Remove(key);
        }
    }
    
    #endregion

    #region 关系变化事件
    /// <summary>
    /// 判断NPC是否会发生争吵
    /// </summary>
    public bool WillNPCsFight(NPC npc1, NPC npc2)
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
    
    /// <summary>
    /// 处理争吵结果
    /// </summary>
    public int ProcessFight(NPC npc1, NPC npc2)
    {
        int penalty = data.config.fightRelationshipPenalty;
        
        // 应用好感度变化
        DecreaseRelationship(npc1, npc2, penalty);
        DecreaseRelationship(npc2, npc1, penalty);

        return penalty;
    }
    
    /// <summary>
    /// 处理友好聊天结果
    /// </summary>
    public int ProcessFriendlyChat(NPC npc1, NPC npc2)
    {
        int bonus = data.config.baseRelationshipChange;
        
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
        IncreaseRelationship(npc1, npc2, bonus);
        IncreaseRelationship(npc2, npc1, bonus);
        
        return bonus;
    }
    
    /// <summary>
    /// 处理共同工作的好感度奖励
    /// </summary>
    public void ProcessWorkTogetherBonus(NPC npc1, NPC npc2)
    {
        if (npc1 == null || npc2 == null || npc1 == npc2) return;
        
        int bonus = data.config.workTogetherBonus;
        
        // 特殊词条影响
        if (npc1.HasTrait(NPCTraitType.SocialMaster) || npc2.HasTrait(NPCTraitType.SocialMaster))
        {
            bonus = Mathf.RoundToInt(bonus * 1.3f);
        }
        
        IncreaseRelationship(npc1, npc2, bonus);
        IncreaseRelationship(npc2, npc1, bonus);
        
        if (NPCManager.Instance.showDebugInfo)
            Debug.Log($"[SocialRelationshipManager] {npc1.data.npcName} 和 {npc2.data.npcName} 因共同工作获得好感度 +{bonus}");
    }
    
    /// <summary>
    /// 处理每日好感度衰减
    /// </summary>
    public void ProcessDailyRelationshipDecay()
    {
        var keysToUpdate = new List<(string, string)>(data.relationships.Keys);
        
        foreach (var key in keysToUpdate)
        {
            int currentValue = data.relationships[key];
            int newValue = currentValue + data.config.relationshipDecayDaily;
            newValue = Mathf.Clamp(newValue, data.config.minRelationship, data.config.maxRelationship);
            data.relationships[key] = newValue;
        }
        
        if (NPCManager.Instance.showDebugInfo)
        {
            Debug.Log($"[SocialRelationshipManager] 处理每日好感度衰减完成，处理了 {keysToUpdate.Count} 个关系的每日衰减");
        }
    }
    #endregion
    /// <summary>
    /// 计算平均好感度
    /// </summary>
    public float CalculateAverageRelationship()
    {
        float totalRelationship = 0f;
        int relationshipCount = 0;
        
        foreach (var key in data.relationships.Keys)
        {
            totalRelationship += data.relationships[key];
            relationshipCount++;
        }
        
        return relationshipCount > 0 ? totalRelationship / relationshipCount : 50f;
    }
    
    #region 私有辅助方法
    
    /// <summary>
    /// 获取性格冲突修正值
    /// </summary>
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
    
    /// <summary>
    /// 获取关系修正值
    /// </summary>
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
    
    /// <summary>
    /// 获取特质修正值
    /// </summary>
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
} 