using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public System.DateTime saveTime;
    public Dictionary<ResourceType, Dictionary<int, int>> resources;
    public List<BuildingSaveData> buildings;
    public List<NPCSaveData> npcs;
    public List<Report> reports;
}

[System.Serializable]
public class BuildingSaveData
{
    public BuildingSubType buildingType;
    public Vector2Int position;
    public int level;
    public List<int> assignedNPCIds;
    public List<EquipmentType> installedEquipment;
}

[System.Serializable]
public class NPCSaveData
{
    public int npcId;
    public NPCData npcData;
    public NPCState currentState;
    public int assignedBuildingId;
    public Dictionary<int, int> relationships;
}