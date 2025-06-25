using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.AI;

/// <summary>
/// 社交系统主协调器 - 重构后的简化版本
/// 负责协调各个子管理器，提供统一的API接口
/// </summary>
public class SocialSystem
{
    #region 管理器
    private SocialSystemData data;
    private SocialCooldownManager cooldownManager;
    private SocialInvitationManager invitationManager;
    private SocialInteractionManager interactionManager;
    private SocialRelationshipManager relationshipManager;
    #endregion
    
    #region 配置参数 (保留向后兼容性)
    [Header("配置文件")]
    [SerializeField] private SocialSystemConfig config;

    // 向后兼容性属性
    public float interactionCooldown => data?.config?.interactionCooldown ?? 0f;
    public float personalSocialCooldown => data?.config?.personalSocialCooldown ?? 0f;
    public float socialInteractionDistance => data?.config?.socialInteractionDistance ?? 2f;
    public float socialMoveSpeed => data?.config?.socialMoveSpeed ?? 0.5f;
    public float socialTimeout => data?.config?.socialTimeout ?? 10f;
    
    // 向后兼容性集合属性
    public Dictionary<(NPC, NPC), float> interactionCooldowns => data?.interactionCooldowns;
    public Dictionary<NPC, float> personalSocialCooldowns => data?.personalSocialCooldowns;
    public Dictionary<NPC, int> dailyInteractionCounts => data?.dailyInteractionCounts;
    public Dictionary<(NPC, NPC), SocialInteraction> activeInteractions => data?.activeInteractions;
    public Dictionary<int, SocialInvitation> activeInvitations => data?.activeInvitations;
    #endregion
    
    #region 初始化
    public void Initialize(List<NPC> npcList, SocialSystemConfig socialConfig = null) 
    { 
        config = socialConfig ?? Resources.Load<SocialSystemConfig>("SocialSystemConfig");
    
        if (config == null)
        {
            Debug.LogError("[SocialSystem] SocialSystemConfig not found! Using default values.");
        }

        // 初始化数据容器
        data = new SocialSystemData();
        data.Initialize(npcList, config);
        
        // 初始化各个管理器
        cooldownManager = new SocialCooldownManager(data);
        relationshipManager = new SocialRelationshipManager(data);
        interactionManager = new SocialInteractionManager(data, cooldownManager, relationshipManager);
        invitationManager = new SocialInvitationManager(data, cooldownManager);
        
        // 初始化邀请管理器的配置
        invitationManager.Initialize(config);

        // 订阅游戏事件
        GameEvents.OnDayChanged += OnDayChanged;
        GameEvents.OnNPCSocialInteractionStarted += HandleSocialInteractionStarted;
        
        if(NPCManager.Instance.showDebugInfo) 
            Debug.Log($"[SocialSystem] 重构版本初始化完成，管理 {data.npcs.Count} 个NPC的社交关系");
    }
    #endregion
    
    #region 主循环更新
    public void UpdateSocialInteractions()
    {
        // 委托给各个管理器进行更新
        interactionManager?.UpdateActiveInteractions();
        cooldownManager?.UpdateCooldowns();
        invitationManager?.UpdateInvitationSystem();
    }
    #endregion
    
    #region 事件处理
    private void OnDayChanged(TimeEventArgs args)
    {
        // 重置每日数据
        cooldownManager?.ResetDailyData();
        
        // 处理好感度衰减
        relationshipManager?.ProcessDailyRelationshipDecay();
    }

    private void HandleSocialInteractionStarted(NPCEventArgs args)
    {
        interactionManager?.StartInteraction(args.npc, args.otherNPC);
    }
    #endregion
    
    #region 邀请系统API
    public bool SendSocialInvitation(NPC sender, NPC receiver) 
    {
        return invitationManager?.SendSocialInvitation(sender, receiver) ?? false;
    }
    
    public void RespondToInvitation(int invitationId, NPC responder, bool accepted, string reason = "") 
    {
        invitationManager?.RespondToInvitation(invitationId, responder, accepted, reason);
    }
    
    public void CancelInvitation(int invitationId, string reason) 
    {
        invitationManager?.CancelInvitation(invitationId, reason);
    }
    
    public SocialInvitation GetPendingInvitation(NPC npc) 
    {
        return invitationManager?.GetPendingInvitation(npc);
    }
    
    public SocialInvitationResponse GetInvitationResponse(NPC sender) 
    {
        return invitationManager?.GetInvitationResponse(sender);
    }
    #endregion
    
    #region 互动管理API
    public NPC GetSocialPartner(NPC npc) 
    {
        return interactionManager?.GetSocialPartner(npc);
    }
    
    public void AddSocialPair(NPC npc1, NPC npc2) 
    {
        interactionManager?.AddSocialPair(npc1, npc2);
    }
    
    public void RemoveSocialPair(NPC npc1, NPC npc2) 
    {
        interactionManager?.RemoveSocialPair(npc1, npc2);
    }
    
    public bool AreSocialPartners(NPC npc1, NPC npc2) 
    {
        return interactionManager?.AreSocialPartners(npc1, npc2) ?? false;
    }
    
    public List<NPC> FindPotentialSocialPartners(NPC npc) 
    {
        return interactionManager?.FindPotentialSocialPartners(npc) ?? new List<NPC>();
    }
    
    public NPC FindNearestSocialPartner(NPC npc) 
    {
        return interactionManager?.FindNearestSocialPartner(npc);
    }
    
    public SocialPositions CalculateSocialPositions(NPC npc1, NPC npc2) 
    {
        return interactionManager?.CalculateSocialPositions(npc1, npc2) ?? default;
    }
    #endregion
    
    #region 关系管理API
    public void ProcessWorkTogetherBonus(NPC npc1, NPC npc2) 
    {
        relationshipManager?.ProcessWorkTogetherBonus(npc1, npc2);
    }
    #endregion
    
    #region 统计和调试
    public SocialSystemStats GetStats()
    {
        return new SocialSystemStats
        {
            totalNPCs = data?.npcs?.Count ?? 0,
            activeInteractions = data?.activeInteractions?.Count ?? 0,
            averageRelationship = relationshipManager?.CalculateAverageRelationship() ?? 50f,
            dailyInteractionsTotal = data?.dailyInteractionCounts?.Values.Sum() ?? 0
        };
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
    public InvitationPriority priority;
    public float senderRelationship; // 发送者与接收者的关系值，用于优先级判断

    public bool IsExpired => Time.time > expireTime;
    public bool IsValid => status == SocialInvitationStatus.Pending && !IsExpired;
    
    public SocialInvitation(int id, NPC sender, NPC receiver, SocialPositions suggestedSocialLocaiton, float sendTime, float expireTime, SocialInvitationStatus status){
        this.invitationId = id;
        this.sender = sender;
        this.receiver = receiver;
        this.suggestedSocialLocaiton = suggestedSocialLocaiton;
        this.sendTime = sendTime;
        this.expireTime = expireTime;
        this.status = status;
        this.priority = CalculatePriority(sender.GetRelationshipWith(receiver));
        this.senderRelationship = sender.GetRelationshipWith(receiver);
    }
    
    private InvitationPriority CalculatePriority(float relationship)
    {
        if (relationship >= 80) return InvitationPriority.High;
        if (relationship >= 50) return InvitationPriority.Normal;
        return InvitationPriority.Low;
    }
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