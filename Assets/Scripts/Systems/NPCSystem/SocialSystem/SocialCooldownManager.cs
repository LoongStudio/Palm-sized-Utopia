using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 社交冷却时间管理器 - 管理所有社交相关的冷却时间
/// </summary>
public class SocialCooldownManager
{
    private SocialSystemData data;
    
    public SocialCooldownManager(SocialSystemData systemData)
    {
        data = systemData;
    }
    
    /// <summary>
    /// 更新所有冷却时间
    /// </summary>
    public void UpdateCooldowns()
    {
        UpdateInteractionCooldowns();
        UpdatePersonalSocialCooldowns();
    }
    
    /// <summary>
    /// 更新NPC对的交互冷却时间
    /// </summary>
    private void UpdateInteractionCooldowns()
    {
        if (data.interactionCooldowns == null) return;
        
        var keys = data.interactionCooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            data.interactionCooldowns[key] -= Time.deltaTime;
            if (data.interactionCooldowns[key] <= 0)
            {
                data.interactionCooldowns.Remove(key);
            }
        }
    }
    
    /// <summary>
    /// 更新个人社交冷却时间
    /// </summary>
    private void UpdatePersonalSocialCooldowns()
    {
        if (data.personalSocialCooldowns == null) return;
        
        var personalKeys = data.personalSocialCooldowns.Keys.ToList();
        foreach (var key in personalKeys)
        {
            data.personalSocialCooldowns[key] -= Time.deltaTime;
            if (data.personalSocialCooldowns[key] <= 0)
            {
                data.personalSocialCooldowns.Remove(key);
            }
        }
    }
    
    /// <summary>
    /// 设置互动冷却时间
    /// </summary>
    public void SetInteractionCooldown(NPC npc1, NPC npc2, float cooldownTime)
    {
        var key = data.GetStandardizedPair(npc1, npc2);
        data.interactionCooldowns[key] = cooldownTime;
    }
    
    /// <summary>
    /// 设置个人社交冷却时间
    /// </summary>
    public void SetPersonalSocialCooldown(NPC npc, float cooldownTime)
    {
        data.personalSocialCooldowns[npc] = cooldownTime;
    }
    
    /// <summary>
    /// 检查是否在冷却时间内
    /// </summary>
    public bool IsInCooldown(NPC npc1, NPC npc2)
    {
        var key = data.GetStandardizedPair(npc1, npc2);
        return data.interactionCooldowns.ContainsKey(key) && data.interactionCooldowns[key] > 0;
    }
    
    /// <summary>
    /// 检查个人社交是否在冷却时间内
    /// </summary>
    public bool IsPersonalSocialInCooldown(NPC npc)
    {
        return data.personalSocialCooldowns.ContainsKey(npc) && data.personalSocialCooldowns[npc] > 0;
    }
    
    /// <summary>
    /// 重置每日数据
    /// </summary>
    public void ResetDailyData()
    {
        data.dailyInteractionCounts.Clear();
    }
    
    /// <summary>
    /// 获取每日互动次数
    /// </summary>
    public int GetDailyInteractionCount(NPC npc)
    {
        return data.dailyInteractionCounts.GetValueOrDefault(npc, 0);
    }
    
    /// <summary>
    /// 增加每日互动计数
    /// </summary>
    public void IncrementDailyInteractionCount(NPC npc)
    {
        if (!data.dailyInteractionCounts.ContainsKey(npc))
        {
            data.dailyInteractionCounts[npc] = 0;
        }
        data.dailyInteractionCounts[npc]++;
    }
} 