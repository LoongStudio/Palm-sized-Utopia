using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 社交邀请管理器 - 管理所有社交邀请相关逻辑
/// </summary>
public class SocialInvitationManager
{
    private SocialSystemData data;
    private SocialCooldownManager cooldownManager;
    private float invitationTimeout = 5f;
    private int maxPendingInvitations = 3;
    
    public SocialInvitationManager(SocialSystemData systemData, SocialCooldownManager cooldownMgr)
    {
        data = systemData;
        cooldownManager = cooldownMgr;
    }
    
    public void Initialize(SocialSystemConfig config)
    {
        invitationTimeout = config.invitationTimeout;
        maxPendingInvitations = config.maxPendingInvitations;
    }
    
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
        var socialPositions = CalculateSocialPositions(sender, receiver);
        var invitation = new SocialInvitation(data.nextInvitationId++, sender, receiver, socialPositions,
         Time.time, Time.time + invitationTimeout, SocialInvitationStatus.Pending);
        
        // 3. 处理邀请冲突（如果接收者也在PrepareForSocial状态）
        HandleInvitationConflict(invitation);
        
        // 4. 注册邀请
        data.activeInvitations[invitation.invitationId] = invitation;
        
        if (NPCManager.Instance.showDebugInfo)
            Debug.Log($"[SocialInvitationManager] {sender.data.npcName} 向 {receiver.data.npcName} 发送社交邀请 (ID: {invitation.invitationId})");
        
        return true;
    }
    
    /// <summary>
    /// 响应社交邀请
    /// </summary>
    public void RespondToInvitation(int invitationId, NPC responder, bool accepted, string reason = "")
    {
        if (!data.activeInvitations.TryGetValue(invitationId, out var invitation))
        {
            Debug.LogWarning($"[SocialInvitationManager] 邀请 {invitationId} 不存在或已处理");
            return;
        }
        
        if (invitation.receiver != responder)
        {
            Debug.LogError($"[SocialInvitationManager] 响应者 {responder.data.npcName} 不是邀请 {invitationId} 的接收者");
            return;
        }
        
        if (!invitation.IsValid)
        {
            Debug.LogWarning($"[SocialInvitationManager] 邀请 {invitationId} 已过期或无效");
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
        data.pendingResponses.Enqueue(response);
        
        if (NPCManager.Instance.showDebugInfo)
            Debug.Log($"[SocialInvitationManager] {responder.data.npcName} {(accepted ? "接受" : "拒绝")}了邀请 {invitationId} {reason}");
    }
    
    /// <summary>
    /// 取消邀请
    /// </summary>
    public void CancelInvitation(int invitationId, string reason)
    {
        if (data.activeInvitations.TryGetValue(invitationId, out var invitation))
        {
            invitation.status = SocialInvitationStatus.Declined;
            
            // 创建自动拒绝响应
            var response = new SocialInvitationResponse
            {
                invitationId = invitationId,
                responder = invitation.receiver,
                accepted = false,
                reason = $"邀请被系统取消: {reason}"
            };
            
            data.pendingResponses.Enqueue(response);
            
            if (NPCManager.Instance.showDebugInfo)
                Debug.Log($"[SocialInvitationManager] 邀请 {invitationId} 被取消: {reason}");
        }
    }
    
    /// <summary>
    /// 获取NPC的待处理邀请
    /// </summary>
    public SocialInvitation GetPendingInvitation(NPC npc)
    {
        return data.activeInvitations.Values
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
        var responses = data.pendingResponses.ToArray();
        data.pendingResponses.Clear();
        
        foreach (var response in responses)
        {
            if (data.activeInvitations.TryGetValue(response.invitationId, out var invitation) && 
                invitation.sender == sender)
            {
                // 找到属于该发送者的响应，其他响应放回队列
                foreach (var otherResponse in responses)
                {
                    if (otherResponse != response)
                        data.pendingResponses.Enqueue(otherResponse);
                }
                return response;
            }
        }
        
        // 没找到，所有响应放回队列
        foreach (var response in responses)
        {
            data.pendingResponses.Enqueue(response);
        }
        
        return null;
    }
    
    /// <summary>
    /// 更新邀请系统
    /// </summary>
    public void UpdateInvitationSystem()
    {
        CleanupExpiredInvitations();
    }
    
    /// <summary>
    /// 清理过期邀请
    /// </summary>
    private void CleanupExpiredInvitations()
    {
        var expiredIds = data.activeInvitations.Values
            .Where(inv => inv.IsExpired)
            .Select(inv => inv.invitationId)
            .ToList();
        
        foreach (var id in expiredIds)
        {
            data.activeInvitations.Remove(id);
        }
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
        if (receiver.currentState != NPCState.Idle && receiver.currentState != NPCState.PrepareForSocial)
        {
            return false;
        }
        
        // 3. 检查是否已有待处理邀请
        bool hasExistingInvitation = data.activeInvitations.Values.Any(inv => 
            (inv.sender == sender && inv.receiver == receiver || 
             inv.sender == receiver && inv.receiver == sender) && 
            inv.IsValid);
        
        if (hasExistingInvitation)
        {
            return false;
        }
        
        // 4. 检查社交条件（距离、冷却时间等）
        if (!CanInteractBasicCheck(sender, receiver))
        {
            return false;
        }
        
        // 5. 检查接收者待处理邀请数量
        int receiverPendingCount = data.activeInvitations.Values.Count(inv => 
            inv.receiver == receiver && inv.IsValid);
        
        if (receiverPendingCount >= maxPendingInvitations)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 基本的互动条件检查
    /// </summary>
    private bool CanInteractBasicCheck(NPC npc1, NPC npc2)
    {
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
        
        // 检查休息时间
        if (npc1.IsRestTime() || npc2.IsRestTime())
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 处理邀请冲突
    /// </summary>
    private void HandleInvitationConflict(SocialInvitation newInvitation)
    {
        var receiver = newInvitation.receiver;
        
        // 如果接收者在PrepareForSocial状态，检查是否有更好的选择
        if (receiver.currentState == NPCState.PrepareForSocial)
        {
            // 查找接收者当前发出的邀请
            var receiverSentInvitation = data.activeInvitations.Values
                .FirstOrDefault(inv => inv.sender == receiver && inv.IsValid);
            
            if (receiverSentInvitation != null)
            {
                // 比较优先级：新邀请 vs 接收者已发出的邀请
                bool shouldAcceptNewInvitation = ShouldPreferNewInvitation(newInvitation, receiverSentInvitation);
                
                if (shouldAcceptNewInvitation)
                {
                    // 取消接收者之前发出的邀请
                    CancelInvitation(receiverSentInvitation.invitationId, "收到更优先的邀请");
                }
            }
        }
    }
    
    /// <summary>
    /// 判断是否应该优先选择新邀请
    /// </summary>
    private bool ShouldPreferNewInvitation(SocialInvitation newInvitation, SocialInvitation existingInvitation)
    {
        // 1. 优先级比较
        if (newInvitation.priority > existingInvitation.priority)
            return true;
        if (newInvitation.priority < existingInvitation.priority)
            return false;
        
        // 2. 相同优先级时，比较关系值
        float newRelationship = newInvitation.senderRelationship;
        float existingRelationship = existingInvitation.receiver.GetRelationshipWith(existingInvitation.sender);
        
        if (Mathf.Abs(newRelationship - existingRelationship) > 10f)
        {
            return newRelationship > existingRelationship;
        }
        
        // 3. 关系值相近时，比较时间（谁先发邀请）
        return newInvitation.sendTime < existingInvitation.sendTime;
    }
    
    /// <summary>
    /// 计算社交位置
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
} 