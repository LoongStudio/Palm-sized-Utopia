using System.Collections.Generic;
using UnityEngine;

public class BuildingEventArgs 
{
    public Building building;
    public IPlaceable placeable;
    public BuildingEventType eventType;
    public List<Vector2Int> positions;
    public int oldLevel;
    public int newLevel;
    public bool isSuccess;
    public System.DateTime timestamp;
    
    public enum BuildingEventType 
    {
        Created,
        PlaceSuccess,
        PlaceFailed,
        Upgraded,
        Destroyed,
        NPCAssigned,
        NPCRemoved,
        EquipmentInstalled,
        EquipmentRemoved
    }
}