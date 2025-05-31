using System.Collections.Generic;
using UnityEngine;
public abstract class BuildingBase : MonoBehaviour
{
    [Header("建筑本身的属性")]
    public int buildingID;
    public BuildingBaseType baseType;
    public BuildingSubType subType;
    public GameObject nextLevelPrefab;
    
    [Header("NPC交互")]
    public List<NPCData> npcData;
    
    [Header("建造过程模拟")]
    public float buildingTime;
    private float _currentTime;
    public bool isBuildingComplete;
    
    // public abstract void UpdateState();
}