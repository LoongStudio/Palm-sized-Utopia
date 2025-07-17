using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class BuildingData
{
    [FoldoutGroup("基本信息"),LabelText("名称")] public string buildingName;
    [FoldoutGroup("基本信息"),LabelText("描述")] public string description;
    [FoldoutGroup("基本信息"),LabelText("图标")] public Sprite icon;

    [FoldoutGroup("建筑信息"),LabelText("类型")] public BuildingType buildingType;
    [FoldoutGroup("建筑信息"),LabelText("子类型")] public BuildingSubType subType;
    [FoldoutGroup("建筑信息"),LabelText("购买价格")] public int purchasePrice;
    [FoldoutGroup("建筑信息"),LabelText("升级价格")] public int[] upgradePrices;
    [FoldoutGroup("建筑信息"),LabelText("最大等级")] public int maxLevel;
    [FoldoutGroup("建筑信息"),LabelText("大小")] public Vector2Int size;
    [FoldoutGroup("建筑信息"),LabelText("NPC槽位数量")] public int npcSlots;
    [FoldoutGroup("建筑信息"),LabelText("设备槽位数量")] public int equipmentSlots;
    [FoldoutGroup("建筑信息"),LabelText("基础效率")] public float baseEfficiency;
    [FoldoutGroup("建筑信息"),LabelText("建筑适配权重")] public BuildingFitWeightData fitWeightData;
} 

[System.Serializable]
public class ProductionBuildingData
{
    [FoldoutGroup("资源相关"),LabelText("默认资源数量")] public List<ResourceStack> defaultResources;
    [FoldoutGroup("资源相关"),LabelText("默认最大值")] public int defaultMaxValue;
    [FoldoutGroup("资源相关"),LabelText("可放入资源")] public List<ResourceConfig> acceptResources;
    [FoldoutGroup("转换规则"),LabelText("生产规则")] public List<ConversionRule> conversionRules;
    [FoldoutGroup("转换规则"),LabelText("默认接收模式")] public Inventory.InventoryAcceptMode defaultAcceptMode;
    [FoldoutGroup("转换规则"),LabelText("默认过滤模式")] public Inventory.InventoryListFilterMode defaultFilterMode;
    [FoldoutGroup("转换规则"),LabelText("默认白名单")] public List<ResourceConfig> defaultAcceptList;
    [FoldoutGroup("转换规则"),LabelText("默认黑名单")] public List<ResourceConfig> defaultRejectList;
}

[System.Serializable]
public class BuildingFitWeightData
{
    [FoldoutGroup("权重参数"),LabelText("插槽权重")] public float weightSlot = 0.5f;
    [FoldoutGroup("权重参数"),LabelText("资源输出权重")] public float weightResourceAgainst = 0.5f;
    [FoldoutGroup("权重参数"),LabelText("资源需求权重")] public float weightResourceInvolved = 0.8f;
    [FoldoutGroup("权重参数"),LabelText("权重有效阈值")] public float weightThreshold = 0.1f;
}