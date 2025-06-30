using System;
using System.Collections.Generic;

[System.Serializable]
public class NPCData
{
    public string npcId;
    public string npcName;
    public int baseSalary;
    public int restTimeStart;
    public int restTimeEnd;
    public int workTimeStart;
    public int workTimeEnd;
    public int baseWorkAbility;
    public int itemCapacity;
    public int itemTakeEachTimeCapacity;
    public NPCPersonalityType personality;
    public List<NPCTraitType> traits;

    public override string ToString()
    {
        var traitsString = traits != null && traits.Count > 0 
            ? string.Join(", ", traits) 
            : "无";
            
        return $"NPC信息:\n" +
               $"  ID: {npcId}\n" +
               $"  姓名: {npcName}\n" +
               $"  基础工资: {baseSalary}\n" +
               $"  基础工作能力: {baseWorkAbility}\n" +
               $"  物品容量: {itemCapacity}\n" +
               $"  休息时间: {restTimeStart}:00 - {restTimeEnd}:00\n" +
               $"  工作时间: {workTimeStart}:00 - {workTimeEnd}:00\n" +
               $"  性格: {personality}\n" +
               $"  词条: {traitsString}";
    }

} 