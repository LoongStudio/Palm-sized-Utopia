// ResourceData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/ResourceData")]
public class ResourceData : ScriptableObject
{
    [Header("基本信息")]
    public ResourceType resourceType;
    public string resourceName;
    public string description;
    public Sprite icon;
    
    [Header("经济属性")]
    public int basePrice = 1;           // 基础价格
    public bool canBeSold = true;       // 是否可以出售
    public bool canBeBought = true;     // 是否可以购买
    
    [Header("转化属性")]
    public bool canBeConverted = false; // 是否可以转化
    public ResourceConversion[] conversions; // 转化配方
    
    [Header("存储属性")]
    public int stackLimit = 999;        // 堆叠上限
    public bool hasExpiration = false;  // 是否有过期时间
    public float expirationTime = 0;    // 过期时间（小时）

    
}

[System.Serializable]
public class ResourceConversion
{
    public ResourceType outputType;     // 产出资源类型
    public int outputAmount;            // 产出数量
    public float conversionTime;        // 转化时间（秒）
    public ResourceCost[] inputCosts;   // 输入成本
}

[System.Serializable]
public class ResourceCost
{
    public ResourceType resourceType;
    public int amount;
}