using System;
using System.Collections.Generic;

[System.Serializable]
public class NPCData
{
    public string npcName;
    public int baseSalary;
    public int restTimeStart;
    public int restTimeEnd;
    public int workTimeStart;
    public int workTimeEnd;
    public int baseWorkAbility;
    public int itemCapacity;
    public NPCPersonalityType personality;
    public List<NPCTraitType> traits;

} 