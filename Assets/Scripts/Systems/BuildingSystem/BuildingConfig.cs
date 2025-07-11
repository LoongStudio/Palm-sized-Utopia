using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BuildingConfig", menuName = "Utopia/BuildingConfig")]
public class BuildingConfig : ScriptableObject
{
    public List<BuildingPrefabData> buildingPrefabDatas;
}

[System.Serializable]
public class BuildingPrefabData
{
    [FoldoutGroup("建筑信息"),LabelText("子类型")] public BuildingSubType subType;
    [FoldoutGroup("建筑信息"),LabelText("建筑数据")] public BuildingData buildingDatas;
    [FoldoutGroup("建筑信息"),LabelText("生产信息")] public ProductionBuildingData productionBuildingDatas;
    [FoldoutGroup("建筑信息"),LabelText("预制体")] public GameObject prefab;
}