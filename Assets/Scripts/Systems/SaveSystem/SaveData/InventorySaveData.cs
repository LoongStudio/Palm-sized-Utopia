using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySaveData : GameSaveData
{
    public Inventory.InventoryOwnerType ownerType;
    public List<ResourceStackSaveData> currentStacks;  // 当前资源堆栈
    public Inventory.InventoryAcceptMode acceptMode;             // 接收模式
    public Inventory.InventoryListFilterMode filterMode;         // 过滤模式
    public Dictionary<ResourceType, int> acceptList;                 // 白名单（ResourceConfig的GUID）
    public Dictionary<ResourceType, int> rejectList;                 // 黑名单（ResourceConfig的GUID）
    public int defaultMaxValue;
}

[System.Serializable]
public class ResourceStackSaveData : GameSaveData
{
    public ResourceType type;
    public int subType;
    public int amount;                 // 当前数量
    public int storageLimit;           // 存储上限
}