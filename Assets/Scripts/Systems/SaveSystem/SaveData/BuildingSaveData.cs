using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingSaveData : GameSaveData
{
    public List<BuildingInstanceSaveData> buildings; // 建筑实例数据
}

[System.Serializable]
public class BuildingInstanceSaveData : GameSaveData
{
    // 基础信息
    public string buildingId;                    // 建筑唯一ID
    public BuildingSubType subType;              // 建筑子类型
    public BuildingStatus status;                // 建筑状态
    public int currentLevel;                     // 当前等级
    public List<Vector2Int> positions;           // 建筑位置

    // 背包系统
    public InventorySaveData inventory;          // 建筑背包数据

    // 槽位管理
    public List<EquipmentSaveData> installedEquipment; // 安装的设备

    // 
}


[System.Serializable]
public class EquipmentSaveData : GameSaveData
{
    public EquipmentType equipmentType;  // 设备类型
}