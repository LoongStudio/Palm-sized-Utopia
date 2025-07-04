using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCSaveData : GameSaveData
{
    public List<NPCInstanceSaveData> npcInstances; // 所有NPC实例数据
    public SocialSystemSaveData socialSystemSaveData; // 社交系统数据
}

[System.Serializable]
public class NPCInstanceSaveData : GameSaveData
{
    public string npcId; // NPC的唯一ID
    public NPCData npcData; // NPC的配置数据
    public InventorySaveData inventorySaveData; // NPC的背包数据
    // 重写ToString方法，展示ID，叫什么名字
    public override string ToString() { return $"NPCInstanceSaveData: {npcId} {npcData.npcName}"; }
}
[System.Serializable]
public class SocialSystemSaveData : GameSaveData
{
    public Dictionary<(string, string), int> relationships; // NPC之间的关系
    public Dictionary<(string, string), float> interactionCooldowns; // NPC之间的互动冷却时间
    public Dictionary<string, float> personalSocialCooldowns; // NPC的个性化社交冷却时间
    public Dictionary<string, int> dailyInteractionCounts; // NPC的每日互动次数
    // 重写ToString方法，展示有多少个NPC管理数据
    public override string ToString() { return $"SocialSystemSaveData: {relationships.Count}对NPC关系 {interactionCooldowns.Count}对NPC互动冷却时间 {personalSocialCooldowns.Count}名NPC个性化社交冷却时间 {dailyInteractionCounts.Count}名NPC每日互动次数"; }
}