// ReportData.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReportData
{
    [Header("报告基本信息")]
    public DateTime reportDate;
    public string reportTitle;
    public float reportDuration; // 报告时长（小时）
    
    [Header("资源变化")]
    public Dictionary<ResourceType, int> resourcesProduced;
    public Dictionary<ResourceType, int> resourcesConsumed;
    public Dictionary<ResourceType, int> resourcesNet; // 净变化
    
    [Header("经济数据")]
    public int goldEarned;
    public int goldSpent;
    public int netIncome; // 净收入
    
    [Header("建筑活动")]
    public List<BuildingActivityRecord> buildingActivities;
    
    [Header("NPC数据")]
    public int totalNPCs;
    public float averageFavorability;
    public int npcProductivityScore; // NPC生产力评分
    
    public ReportData()
    {
        reportDate = DateTime.Now;
        resourcesProduced = new Dictionary<ResourceType, int>();
        resourcesConsumed = new Dictionary<ResourceType, int>();
        resourcesNet = new Dictionary<ResourceType, int>();
        buildingActivities = new List<BuildingActivityRecord>();

        
    }
    
    // 计算净资源变化
    public void CalculateNetResources()
    {
        resourcesNet.Clear();
        
        // 遍历所有资源类型
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int produced = resourcesProduced.ContainsKey(type) ? resourcesProduced[type] : 0;
            int consumed = resourcesConsumed.ContainsKey(type) ? resourcesConsumed[type] : 0;
            resourcesNet[type] = produced - consumed;
        }
        
        netIncome = goldEarned - goldSpent;
    }
}

[System.Serializable]
public class BuildingActivityRecord
{
    public BuildingType buildingType;
    public string buildingName;
    public BuildingAction action;
    public DateTime timestamp;
    public int costOrValue; // 建造/升级花费或产出价值
    
    public BuildingActivityRecord(BuildingType type, string name, BuildingAction action, int cost = 0)
    {
        this.buildingType = type;
        this.buildingName = name;
        this.action = action;
        this.timestamp = DateTime.Now;
        this.costOrValue = cost;
    }
}

public enum BuildingAction
{
    Built,      // 建造
    Upgraded,   // 升级
    Produced,   // 生产
    Demolished  // 拆除
}