using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 社交系统数据容器 - 存储所有社交相关的数据
/// </summary>
public class SocialSystemData
{
    [Header("NPC列表")]
    public List<NPC> npcs;
    
    [Header("社交伙伴和互动")]
    public HashSet<(NPC, NPC)> socialPairs = new HashSet<(NPC, NPC)>();
    public Dictionary<(NPC, NPC), SocialInteraction> activeInteractions = new Dictionary<(NPC, NPC), SocialInteraction>();
    public Dictionary<(string, string), int> relationships = new Dictionary<(string, string), int>();
    
    [Header("冷却时间")]
    public Dictionary<(NPC, NPC), float> interactionCooldowns = new Dictionary<(NPC, NPC), float>();
    public Dictionary<NPC, float> personalSocialCooldowns = new Dictionary<NPC, float>();
    
    [Header("每日统计")]
    public Dictionary<NPC, int> dailyInteractionCounts = new Dictionary<NPC, int>();
    
    [Header("邀请系统")]
    public Dictionary<int, SocialInvitation> activeInvitations = new Dictionary<int, SocialInvitation>();
    public Queue<SocialInvitationResponse> pendingResponses = new Queue<SocialInvitationResponse>();
    public int nextInvitationId = 1;
    
    [Header("协程管理")]
    public Dictionary<(NPC, NPC), Coroutine> activeSocialCoroutines = new Dictionary<(NPC, NPC), Coroutine>();
    
    [Header("配置")]
    public SocialSystemConfig config;
    
    public void Initialize(List<NPC> npcList, SocialSystemConfig socialConfig)
    {
        npcs = npcList ?? new List<NPC>();
        config = socialConfig;
        
        // 初始化所有字典
        socialPairs.Clear();
        activeInteractions.Clear();
        interactionCooldowns.Clear();
        personalSocialCooldowns.Clear();
        dailyInteractionCounts.Clear();
        activeInvitations.Clear();
        pendingResponses.Clear();
        activeSocialCoroutines.Clear();
        
        nextInvitationId = 1;
    }
    
    /// <summary>
    /// 获取标准化的NPC对键
    /// </summary>
    public (NPC, NPC) GetStandardizedPair(NPC npc1, NPC npc2)
    {
        return npc1.GetInstanceID() < npc2.GetInstanceID() ? (npc1, npc2) : (npc2, npc1);
    }
} 