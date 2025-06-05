using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public BuildingType buildingType;
    public BuildingSubType subType;
    public string buildingName;
    public int purchasePrice;
    public int[] upgradePrices;
    public int maxLevel;
    public Vector2Int size;
    public int npcSlots;
    public int equipmentSlots;
    public float baseEfficiency;
} 