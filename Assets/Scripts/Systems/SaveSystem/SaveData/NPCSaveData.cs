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
}
[System.Serializable]
public class SocialSystemSaveData : GameSaveData
{
    public Dictionary<(string, string), int> relationships; // NPC之间的关系
    public Dictionary<(string, string), float> interactionCooldowns; // NPC之间的互动冷却时间
    public Dictionary<string, float> personalSocialCooldowns; // NPC的个性化社交冷却时间
    public Dictionary<string, int> dailyInteractionCounts; // NPC的每日互动次数
}