using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 社交关系管理器 - 管理所有好感度和关系相关逻辑
/// </summary>
public class SocialRelationshipManager
{
    private SocialSystemData data;
    
    public SocialRelationshipManager(SocialSystemData systemData)
    {
        data = systemData;
    }
    
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
        npc1.DecreaseRelationship(npc2, penalty);
        npc2.DecreaseRelationship(npc1, penalty);
        
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
        npc1.IncreaseRelationship(npc2, bonus);
        npc2.IncreaseRelationship(npc1, bonus);
        
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
        
        npc1.IncreaseRelationship(npc2, bonus);
        npc2.IncreaseRelationship(npc1, bonus);
        
        if (NPCManager.Instance.showDebugInfo)
            Debug.Log($"[SocialRelationshipManager] {npc1.data.npcName} 和 {npc2.data.npcName} 因共同工作获得好感度 +{bonus}");
    }
    
    /// <summary>
    /// 处理每日好感度衰减
    /// </summary>
    public void ProcessDailyRelationshipDecay()
    {
        foreach (var npc in data.npcs)
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
                    npc.DecreaseRelationship(otherNPC, -data.config.relationshipDecayDaily);
                }
            }
        }
        
        if (NPCManager.Instance.showDebugInfo)
            Debug.Log("[SocialRelationshipManager] 处理每日好感度衰减完成");
    }
    
    /// <summary>
    /// 计算平均好感度
    /// </summary>
    public float CalculateAverageRelationship()
    {
        float totalRelationship = 0f;
        int relationshipCount = 0;
        
        foreach (var npc in data.npcs)
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