using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingConfig", menuName = "Utopia/BuildingConfig")]
public class BuildingConfig : ScriptableObject
{
    public List<BuildingPrefabData> buildingPrefabDatas;
}

[System.Serializable]
public class BuildingPrefabData
{
    public BuildingSubType subType;
    public BuildingData buildingDatas;
    public GameObject prefab;
}