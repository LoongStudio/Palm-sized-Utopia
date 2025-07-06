using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlacaebleSaveData: GameSaveData
{
    public List<PlaceableInstanceSaveData> PlaceableInstances;
}

[System.Serializable]
public class PlaceableInstanceSaveData: GameSaveData
{
    public PlaceableType placeableType;
    public Vector3Int customSize;
    public Vector3Int[] positions;
    public bool isPlaced;
}

