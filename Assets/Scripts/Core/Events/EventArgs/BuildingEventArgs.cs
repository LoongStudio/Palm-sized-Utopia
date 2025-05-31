using UnityEngine;

public class BuildingEventArgs 
{
    public Building building;
    public BuildingEventType eventType;
    public Vector2Int position;
    public int oldLevel;
    public int newLevel;
    public System.DateTime timestamp;
    
    public enum BuildingEventType 
    {
        Built,
        Upgraded,
        Destroyed,
        NPCAssigned,
        NPCRemoved,
        EquipmentInstalled,
        EquipmentRemoved
    }
}