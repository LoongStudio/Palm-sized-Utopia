using System;
using System.Collections.Generic;

[System.Serializable]
public class NPCData
{
    public string npcName;
    public int baseSalary;
    public TimeSpan restTimeStart;
    public TimeSpan restTimeEnd;
    public TimeSpan workTimeStart;
    public TimeSpan workTimeEnd;
    public int baseWorkAbility;
    public int farmingSkill;
    public int livestockSkill;
    public int managementSkill;
    public NPCPersonalityType personality;
    public List<NPCTraitType> traits;
} 