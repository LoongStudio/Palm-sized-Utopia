// ReportManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportManager : SingletonManager<ReportManager>
{
    [Header("报告配置")]
    [SerializeField] private float reportInterval = 24f; // 24小时生成一次报告
    [SerializeField] private int maxHistoryReports = 30; // 最多保存30天历史报告
    
    [Header("当前数据追踪")]
    private ReportData currentReport;
    private DateTime lastReportTime;
    
    [Header("历史报告")]
    [SerializeField] private List<ReportData> historicalReports;
    
    // 实时数据追踪
    private Dictionary<ResourceType, int> dailyResourceProduction;
    private Dictionary<ResourceType, int> dailyResourceConsumption;
    private int dailyGoldEarned;
    private int dailyGoldSpent;
    
    // 事件
    public event System.Action<ReportData> OnReportGenerated;
    public event System.Action<ReportData> OnDailyReportUpdate;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeReportSystem();
    }
    
    private void InitializeReportSystem()
    {
        historicalReports = new List<ReportData>();
    
        // 初始化追踪字典
        dailyResourceProduction = new Dictionary<ResourceType, int>();
        dailyResourceConsumption = new Dictionary<ResourceType, int>();
        
        // 安全地初始化资源追踪
        InitializeResourceTracking();
        
        // 最后重置当前报告
        ResetCurrentReport();
        lastReportTime = DateTime.Now;
    }

    private void InitializeResourceTracking()
    {
        try
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                dailyResourceProduction[type] = 0;
                dailyResourceConsumption[type] = 0;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"初始化资源追踪时出错: {e.Message}");
            // 手动初始化已知的资源类型
            dailyResourceProduction[ResourceType.Seed] = 0;
            dailyResourceProduction[ResourceType.Crop] = 0;
            dailyResourceProduction[ResourceType.Feed] = 0;
            dailyResourceProduction[ResourceType.BreedingAnimal] = 0;
            dailyResourceProduction[ResourceType.Livestock] = 0;
            dailyResourceProduction[ResourceType.Gold] = 0;
            dailyResourceProduction[ResourceType.RewardTicket] = 0;
            
            dailyResourceConsumption[ResourceType.Seed] = 0;
            dailyResourceConsumption[ResourceType.Crop] = 0;
            dailyResourceConsumption[ResourceType.Feed] = 0;
            dailyResourceConsumption[ResourceType.BreedingAnimal] = 0;
            dailyResourceConsumption[ResourceType.Livestock] = 0;
            dailyResourceConsumption[ResourceType.Gold] = 0;
            dailyResourceConsumption[ResourceType.RewardTicket] = 0;
        }
    }
    
    public void Initialize()
    {
        // 订阅各种游戏事件
        SubscribeToGameEvents();
        
        // 启动定期报告生成协程
        StartCoroutine(PeriodicReportGeneration());
        
        Debug.Log("报告系统初始化完成");
    }
    
    private void SubscribeToGameEvents()
    {
        // 订阅资源管理器事件
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged += TrackResourceChange;
            ResourceManager.Instance.OnResourceSpent += TrackResourceSpent;
            ResourceManager.Instance.OnResourceGained += TrackResourceGained;
        }
        
        // 订阅建筑管理器事件
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.OnBuildingChanged += TrackBuildingChange;
        }
        
        // 如果有其他系统的事件也在这里订阅
    }
    
    #region 数据追踪方法
    
    private void TrackResourceChange(ResourceType type, int oldAmount, int newAmount)
    {
        // 根据变化量判断是生产还是消耗
        int change = newAmount - oldAmount;
        
        if (change > 0)
        {
            TrackResourceGained(type, change);
        }
        else if (change < 0)
        {
            TrackResourceSpent(type, Mathf.Abs(change));
        }
    }
    
    private void TrackResourceGained(ResourceType type, int amount)
    {
        if (dailyResourceProduction.ContainsKey(type))
        {
            dailyResourceProduction[type] += amount;
        }
        
        // 更新当前报告
        if (currentReport.resourcesProduced.ContainsKey(type))
        {
            currentReport.resourcesProduced[type] += amount;
        }
        else
        {
            currentReport.resourcesProduced[type] = amount;
        }
        
        // 如果是金币，特殊处理
        if (type == ResourceType.Gold)
        {
            dailyGoldEarned += amount;
            currentReport.goldEarned += amount;
        }
        
        Debug.Log($"资源获得追踪: {type} +{amount}");
    }
    
    private void TrackResourceSpent(ResourceType type, int amount)
    {
        if (dailyResourceConsumption.ContainsKey(type))
        {
            dailyResourceConsumption[type] += amount;
        }
        
        // 更新当前报告
        if (currentReport.resourcesConsumed.ContainsKey(type))
        {
            currentReport.resourcesConsumed[type] += amount;
        }
        else
        {
            currentReport.resourcesConsumed[type] = amount;
        }
        
        // 如果是金币，特殊处理
        if (type == ResourceType.Gold)
        {
            dailyGoldSpent += amount;
            currentReport.goldSpent += amount;
        }
        
        Debug.Log($"资源消耗追踪: {type} -{amount}");
    }
    
    private void TrackBuildingChange(Building building, BuildingAction action)
    {
        BuildingActivityRecord record = new BuildingActivityRecord(
            building.type, 
            building.name, 
            action,
            GetBuildingActionCost(building, action)
        );
        
        currentReport.buildingActivities.Add(record);
        
        Debug.Log($"建筑活动追踪: {building.name} - {action}");
    }
    
    private int GetBuildingActionCost(Building building, BuildingAction action)
    {
        // 根据建筑和行为类型返回相应的成本
        // 这里需要根据你的建筑数据配置来实现
        switch (action)
        {
            case BuildingAction.Built:
                if (building.data?.buildCosts != null && building.data.buildCosts.Length > 0)
                {
                    // 计算总建造成本（假设主要是金币成本）
                    // TODO: 根据实际成本计算
                    int totalCost = 0;
                    foreach (var cost in building.data.buildCosts)
                    {
                        if (cost.resourceType == ResourceType.Gold)
                        {
                            totalCost += cost.amount;
                        }
                    }
                    return totalCost;
                }
                return 0;
            case BuildingAction.Upgraded:
                ResourceCost[] upgradeCosts = building.data?.GetUpgradeCost(building.level);
                if (upgradeCosts != null && upgradeCosts.Length > 0)
                {
                    // 计算总升级成本（假设主要是金币成本）
                    // TODO: 根据实际成本计算
                    int totalCost = 0;
                    foreach (var cost in upgradeCosts)
                    {
                        if (cost.resourceType == ResourceType.Gold)
                        {
                            totalCost += cost.amount;
                        }
                    }
                    return totalCost;
                }
                return 0;
            default:
                return 0;
        }
    }
    
    #endregion
    
    #region 报告生成
    
    private IEnumerator PeriodicReportGeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(3600f); // 每小时检查一次
            
            TimeSpan timeSinceLastReport = DateTime.Now - lastReportTime;
            
            if (timeSinceLastReport.TotalHours >= reportInterval)
            {
                GeneratePeriodicReport();
            }
        }
    }
    
    public void GeneratePeriodicReport()
    {
        // 完成当前报告
        FinalizePeriodReport();
        
        // 保存到历史记录
        SaveReportToHistory(currentReport);
        
        // 触发事件
        OnReportGenerated?.Invoke(currentReport);
        
        // 重置为新的报告周期
        ResetCurrentReport();
        
        Debug.Log($"定期报告已生成: {currentReport.reportTitle}");
    }
    
    private void FinalizePeriodReport()
    {
        // 设置报告时间和标题
        currentReport.reportDate = DateTime.Now;
        currentReport.reportTitle = $"农场日报 - {currentReport.reportDate:yyyy年MM月dd日}";
        currentReport.reportDuration = (float)(DateTime.Now - lastReportTime).TotalHours;
        
        // 获取NPC数据
        UpdateNPCData();
        
        // 计算净资源变化
        currentReport.CalculateNetResources();
        
        lastReportTime = DateTime.Now;
    }
    
    private void UpdateNPCData()
    {
        if (NPCManager.Instance != null)
        {
            var npcs = NPCManager.Instance.GetAllNPCs();
            currentReport.totalNPCs = npcs.Count;
            
            if (npcs.Count > 0)
            {
                float totalFavorability = 0f;
                foreach (var npc in npcs)
                {
                    totalFavorability += npc.favorability;
                }
                currentReport.averageFavorability = totalFavorability / npcs.Count;
            }
            
            // 计算生产力评分（这里可以根据你的游戏逻辑来实现）
            currentReport.npcProductivityScore = CalculateProductivityScore();
        }
    }
    
    private int CalculateProductivityScore()
    {
        // 简单的生产力评分算法
        // 可以根据生产的资源数量、NPC好感度等因素计算
        int baseScore = currentReport.totalNPCs * 10;
        float favorabilityBonus = currentReport.averageFavorability * 0.5f;
        
        return Mathf.RoundToInt(baseScore + favorabilityBonus);
    }
    
    private void ResetCurrentReport()
    {
        currentReport = new ReportData();
        // 确保追踪字典已初始化
        if (dailyResourceProduction == null)
        {
            dailyResourceProduction = new Dictionary<ResourceType, int>();
        }
        if (dailyResourceConsumption == null)
        {
            dailyResourceConsumption = new Dictionary<ResourceType, int>();
        }
        // 安全地重置日常追踪数据
        try
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                dailyResourceProduction[type] = 0;
                dailyResourceConsumption[type] = 0;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"重置当前报告时出错: {e.Message}");
            // 如果枚举遍历失败，清空字典并重新初始化
            dailyResourceProduction.Clear();
            dailyResourceConsumption.Clear();
            InitializeResourceTracking();
        }
        
        dailyGoldEarned = 0;
        dailyGoldSpent = 0;
    }
    
    private void SaveReportToHistory(ReportData report)
    {
        historicalReports.Add(report);
        
        // 保持历史记录数量限制
        while (historicalReports.Count > maxHistoryReports)
        {
            historicalReports.RemoveAt(0);
        }
    }
    
    #endregion
    
    #region 公共接口
    
    /// <summary>
    /// 获取当前实时报告
    /// </summary>
    public ReportData GetCurrentReport()
    {
        // 更新实时数据
        UpdateNPCData();
        currentReport.CalculateNetResources();
        
        return currentReport;
    }
    
    /// <summary>
    /// 获取历史报告列表
    /// </summary>
    public List<ReportData> GetHistoricalReports()
    {
        return new List<ReportData>(historicalReports);
    }
    
    /// <summary>
    /// 获取指定日期的报告
    /// </summary>
    public ReportData GetReportByDate(DateTime date)
    {
        return historicalReports.Find(r => r.reportDate.Date == date.Date);
    }
    
    /// <summary>
    /// 手动生成即时报告
    /// </summary>
    public ReportData GenerateInstantReport()
    {
        ReportData instantReport = new ReportData();
        
        // 复制当前数据
        foreach (var kvp in currentReport.resourcesProduced)
        {
            instantReport.resourcesProduced[kvp.Key] = kvp.Value;
        }
        
        foreach (var kvp in currentReport.resourcesConsumed)
        {
            instantReport.resourcesConsumed[kvp.Key] = kvp.Value;
        }
        
        instantReport.goldEarned = currentReport.goldEarned;
        instantReport.goldSpent = currentReport.goldSpent;
        instantReport.buildingActivities = new List<BuildingActivityRecord>(currentReport.buildingActivities);
        
        // 设置即时报告信息
        instantReport.reportDate = DateTime.Now;
        instantReport.reportTitle = $"即时报告 - {instantReport.reportDate:HH:mm:ss}";
        instantReport.reportDuration = (float)(DateTime.Now - lastReportTime).TotalHours;
        
        // 更新NPC数据
        if (NPCManager.Instance != null)
        {
            var npcs = NPCManager.Instance.GetAllNPCs();
            instantReport.totalNPCs = npcs.Count;
            
            if (npcs.Count > 0)
            {
                float totalFavorability = 0f;
                foreach (var npc in npcs)
                {
                    totalFavorability += npc.favorability;
                }
                instantReport.averageFavorability = totalFavorability / npcs.Count;
            }
        }
        
        instantReport.CalculateNetResources();
        
        return instantReport;
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // 取消订阅事件
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged -= TrackResourceChange;
            ResourceManager.Instance.OnResourceSpent -= TrackResourceSpent;
            ResourceManager.Instance.OnResourceGained -= TrackResourceGained;
        }
        
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.OnBuildingChanged -= TrackBuildingChange;
        }
    }
}