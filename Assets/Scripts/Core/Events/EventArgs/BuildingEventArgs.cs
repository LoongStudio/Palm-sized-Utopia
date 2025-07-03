using UnityEngine;

public class BuildingEventArgs 
{
    public Building building;
    public IPlaceable placeable;
    public BuildingEventType eventType;
    public Vector2Int position;
    public int oldLevel;
    public int newLevel;
    public bool isSuccess;
    public System.DateTime timestamp;
    
    public enum BuildingEventType 
    {
        BuiltFromBuy,
        BuiltFromDrag,
        BuiltFromLoad,
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