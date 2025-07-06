using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // 所有保存数据类的基类，留给子类继承
    // 子类重写ToString方法，用于调试
    public override string ToString() { return "GameSaveData"; }
}

[System.Serializable]
public class CoreSaveData : GameSaveData
{
    // 核心保存数据
    public NPCSaveData npcSaveData; // NPC保存数据
    public BuildingSaveData buildingSaveData; // 建筑保存数据
    public PlacaebleSaveData placeableSaveData; // 地皮保存数据
    // TODO: 添加其他核心保存数据诸如资源，建筑，报告、时间等
}

