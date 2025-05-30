using UnityEngine;
public abstract class BuildingBase : MonoBehaviour
{
    public abstract void UpdateState();
    public BuildingBaseType baseType;
    public BuildingSubType subType;
    public float buildingTime;
    private float _currentTime;
    public bool isBuildingComplete;
    public 
}