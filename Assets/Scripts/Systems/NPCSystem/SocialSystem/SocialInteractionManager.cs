using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 社交互动管理器 - 管理所有社交互动相关逻辑
/// </summary>
public class SocialInteractionManager
{
    private SocialSystemData data;
    private SocialCooldownManager cooldownManager;
    private SocialRelationshipManager relationshipManager;
    
    public SocialInteractionManager(SocialSystemData systemData, SocialCooldownManager cooldownMgr, SocialRelationshipManager relationshipMgr)
    {
        data = systemData;
        cooldownManager = cooldownMgr;
        relationshipManager = relationshipMgr;
    }
    
    /// <summary>
    /// 更新活跃的互动
    /// </summary>
    public void UpdateActiveInteractions()
    {
        var completedInteractions = new List<(NPC, NPC)>();
        
        if (data.activeInteractions == null) return;
        foreach (var kvp in data.activeInteractions)
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
            data.activeInteractions.Remove(key);
        }
    }
    
    /// <summary>
    /// 开始社交互动
    /// </summary>
    public void StartInteraction(NPC npc1, NPC npc2)
    {
        if (npc1 == null || npc2 == null) {
            Debug.LogError("[SocialInteractionManager] NPC为空，无法开始社交互动");
            return;
        }
        
        // 检查activeInteractions中是否存在该互动
        var key = data.GetStandardizedPair(npc1, npc2);
        if(!data.activeInteractions.ContainsKey(key)){
            // 如果不存在，则添加该互动
            data.activeInteractions[key] = new SocialInteraction(npc1, npc2, data.config.interactionDuration);
        }
        
        // 增加每日互动计数
        cooldownManager.IncrementDailyInteractionCount(npc1);
        cooldownManager.IncrementDailyInteractionCount(npc2);
        
        // 调试信息
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialInteractionManager] {npc1.data.npcName} 和 {npc2.data.npcName} 开始社交互动");
        
        DebugDrawSocialInteraction(npc1, npc2);
    }
    
    /// <summary>
    /// 完成互动
    /// </summary>
    private void CompleteInteraction(NPC npc1, NPC npc2, SocialInteraction interaction)
    {
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialInteractionManager] {npc1.data.npcName} 和 {npc2.data.npcName} 社交互动结束");
        
        // 计算互动结果
        bool isFight = relationshipManager.WillNPCsFight(npc1, npc2);
        int relationshipChange;
        NPCState shouldChangeStateTo;
        
        if (isFight)
        {
            relationshipChange = relationshipManager.ProcessFight(npc1, npc2);
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialInteractionManager] {npc1.data.npcName} 和 {npc2.data.npcName} 发生了争吵！互相之间好感度下降了 {relationshipChange}");
            shouldChangeStateTo = NPCState.SocialEndFight;
        }
        else
        {
            relationshipChange = relationshipManager.ProcessFriendlyChat(npc1, npc2);
            if(NPCManager.Instance.showDebugInfo) 
                Debug.Log($"[SocialInteractionManager] {npc1.data.npcName} 和 {npc2.data.npcName} 愉快地聊天了！互相之间好感度上升了 {relationshipChange}");
            shouldChangeStateTo = NPCState.SocialEndHappy;
        }
        
        // 设置冷却时间
        cooldownManager.SetInteractionCooldown(npc1, npc2, data.config.interactionCooldown);
        cooldownManager.SetPersonalSocialCooldown(npc1, data.config.personalSocialCooldown);
        cooldownManager.SetPersonalSocialCooldown(npc2, data.config.personalSocialCooldown);
        
        // 删除社交伙伴对
        RemoveSocialPair(npc1, npc2);
        
        // 触发关系变化事件
        var eventArgs = new NPCEventArgs
        {
            npc = npc1,
            eventType = NPCEventArgs.NPCEventType.RelationshipChanged,
            otherNPC = npc2,
            relationshipChange = relationshipChange,
            shouldChangeStateTo = shouldChangeStateTo,
            timestamp = System.DateTime.Now
        };
        GameEvents.TriggerNPCRelationshipChanged(eventArgs);
        GameEvents.TriggerNPCSocialInteractionEnded(eventArgs);
    }
    
    /// <summary>
    /// 检查是否可以互动
    /// </summary>
    public bool CanInteract(NPC npc1, NPC npc2)
    {
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialInteractionManager] 检查两个NPC是否可以互动: {npc1.data.npcName} 和 {npc2.data.npcName}");
        
        // 检查基本条件，仅有Idle状态的NPC才能进行社交互动
        if (npc1 == npc2 || 
        (npc1.currentState != NPCState.Idle && npc1.currentState != NPCState.PrepareForSocial) || 
        (npc2.currentState != NPCState.Idle && npc2.currentState != NPCState.PrepareForSocial))
        {
            return false;
        }
        
        // 检查NavMesh中的实际距离
        float navMeshDistance = CalculateNavMeshDistance(npc1.transform.position, npc2.transform.position);
        if (navMeshDistance > data.config.interactionRadius || navMeshDistance < 0)
        {
            return false;
        }
        
        // 检查冷却时间
        if (cooldownManager.IsInCooldown(npc1, npc2) || 
            cooldownManager.IsPersonalSocialInCooldown(npc1) ||
            cooldownManager.IsPersonalSocialInCooldown(npc2))
        {
            return false;
        }
        
        // 检查每日互动次数
        if (cooldownManager.GetDailyInteractionCount(npc1) >= data.config.maxDailyInteractions || 
            cooldownManager.GetDailyInteractionCount(npc2) >= data.config.maxDailyInteractions)
        {
            return false;
        }
        
        // 检查是否已经在互动中
        if (IsInInteraction(npc1) || IsInInteraction(npc2))
        {
            return false;
        }
        
        // 检查是否已经有正在进行的社交协程
        var socialKey = data.GetStandardizedPair(npc1, npc2);
        if (data.activeSocialCoroutines.ContainsKey(socialKey))
        {
            return false;
        }

        // 检测两个NPC是否并未处在各自的休息时间段
        if(npc1.IsRestTime() || npc2.IsRestTime())
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 为指定NPC找到潜在的社交伙伴
    /// </summary>
    public List<NPC> FindPotentialSocialPartners(NPC npc)
    {
        List<NPC> potentialPartners = new List<NPC>();
        foreach (var otherNPC in data.npcs)
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
    public NPC FindNearestSocialPartner(NPC npc)
    {
        List<NPC> potentialPartners = FindPotentialSocialPartners(npc);
        if (potentialPartners.Count == 0) return null;
        return potentialPartners.OrderBy(p => Vector3.Distance(npc.transform.position, p.transform.position)).First();
    }
    
    /// <summary>
    /// 获取指定NPC的社交伙伴
    /// </summary>
    public NPC GetSocialPartner(NPC npc)
    {
        if (npc == null) return null;
        
        foreach (var pair in data.socialPairs)
        {
            if (pair.Item1 == npc) return pair.Item2;
            if (pair.Item2 == npc) return pair.Item1;
        }
        
        return null;
    }

    /// <summary>
    /// 添加社交伙伴对
    /// </summary>
    public void AddSocialPair(NPC npc1, NPC npc2)
    {
        if (npc1 == null || npc2 == null || npc1 == npc2) return;
        
        var pair = data.GetStandardizedPair(npc1, npc2);
        data.socialPairs.Add(pair);
        
        if (NPCManager.Instance.showDebugInfo)
            Debug.Log($"[SocialInteractionManager] 添加社交伙伴对: {npc1.data.npcName} <-> {npc2.data.npcName}");
    }

    /// <summary>
    /// 移除社交伙伴对
    /// </summary>
    public void RemoveSocialPair(NPC npc1, NPC npc2)
    {
        if (npc1 == null || npc2 == null) return;
        
        var pair = data.GetStandardizedPair(npc1, npc2);
        if (data.socialPairs.Remove(pair))
        {
            if (NPCManager.Instance.showDebugInfo)
                Debug.Log($"[SocialInteractionManager] 移除社交伙伴对: {npc1.data.npcName} <-> {npc2.data.npcName}");
        }
    }

    /// <summary>
    /// 检查两个NPC是否是社交伙伴
    /// </summary>
    public bool AreSocialPartners(NPC npc1, NPC npc2)
    {
        if (npc1 == null || npc2 == null || npc1 == npc2) return false;
        
        var pair = data.GetStandardizedPair(npc1, npc2);
        return data.socialPairs.Contains(pair);
    }
    
    /// <summary>
    /// 计算社交位置
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
        float halfDistance = data.config.socialInteractionDistance * 0.5f;
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
    
    #region 私有辅助方法
    
    /// <summary>
    /// 检查NPC是否在互动中
    /// </summary>
    private bool IsInInteraction(NPC npc)
    {
        return data.activeInteractions.Any(kvp => kvp.Key.Item1 == npc || kvp.Key.Item2 == npc);
    }
    
    /// <summary>
    /// 计算NavMesh中的实际距离
    /// </summary>
    private float CalculateNavMeshDistance(Vector3 startPos, Vector3 endPos)
    {
        NavMeshPath path = new NavMeshPath();
        
        // 尝试计算路径
        if (NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path))
        {
            // 如果路径状态不完整，返回-1表示无法到达
            if (path.status != NavMeshPathStatus.PathComplete)
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
    
    /// <summary>
    /// 找到最近的NavMesh点
    /// </summary>
    private Vector3 FindNearestNavMeshPoint(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return position; // 如果找不到，返回原位置
    }
    
    /// <summary>
    /// 调试绘制社交互动
    /// </summary>
    private void DebugDrawSocialInteraction(NPC npc1, NPC npc2)
    {
        Debug.DrawLine(npc1.transform.position, npc2.transform.position, Color.red, 10f);
    }
    
    #endregion
} 