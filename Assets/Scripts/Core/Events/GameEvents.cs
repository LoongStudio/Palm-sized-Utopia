using UnityEngine;

public static class GameEvents 
{
    #region 事件定义 - Event Definitions
    
    #region 资源相关事件
    public static event System.Action<ResourceEventArgs> OnResourceChanged;
    
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action<ResourceEventArgs> OnResourceAdded;
    // public static event System.Action<ResourceEventArgs> OnResourceRemoved;
    #endregion
    
    #region 建筑相关事件
    public static event System.Action<BuildingEventArgs> OnBuildingBuilt;
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action<BuildingEventArgs> OnBuildingUpgraded;
    // public static event System.Action<BuildingEventArgs> OnBuildingDestroyed;
    #endregion
    
    #region NPC相关事件
    public static event System.Action<NPCEventArgs> OnNPCHired;
    public static event System.Action<NPCEventArgs> OnNPCInstantiated;
    public static event System.Action<NPCEventArgs> OnNPCFired;
    public static event System.Action<NPCEventArgs> OnNPCDestroyed;
    public static event System.Action<NPCEventArgs> OnNPCStateChanged;
    public static event System.Action<NPCEventArgs> OnNPCRelationshipChanged;
    public static event System.Action<NPCEventArgs> OnNPCSocialInteraction;
    public static event System.Action<NPCEventArgs> OnNPCSocialInteractionEnded;
    #endregion
    
    #region 时间相关事件
    // 核心时间事件 (重新设计，避免功能重复)
    public static event System.Action<GameTime> OnTimeChanged;           // 任何时间变化（细粒度）
    public static event System.Action<int> OnHourChanged;                // 小时变化
    public static event System.Action<int> OnDayChanged;                 // 天数变化（包含新一天开始的语义）
    public static event System.Action<int> OnMonthChanged;               // 月份变化
    public static event System.Action<int> OnYearChanged;                // 年份变化（包含新年的语义）
    public static event System.Action<Season> OnSeasonChanged;           // 季节变化
    public static event System.Action<int> OnWeekChanged;                // 周数变化（每7天）
    
    // 特殊时间节点事件
    public static event System.Action OnNightStarted;                   // 夜晚开始
    public static event System.Action OnWorkTimeStarted;                // 工作时间开始
    public static event System.Action OnWorkTimeEnded;                  // 工作时间结束
    public static event System.Action OnRestTimeStarted;                // 休息时间开始
    #endregion
    
    #region 游戏流程事件
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action OnGameStarted;
    // public static event System.Action OnGamePaused;
    #endregion
    
    #region 已废弃事件 (向后兼容)
    [System.Obsolete("请使用OnDayChanged替代，OnDayChanged现在包含新一天开始的语义")]
    public static event System.Action OnDayStarted;
    [System.Obsolete("请使用OnDayChanged替代")]
    public static event System.Action OnDayPassed;
    [System.Obsolete("请使用OnWeekChanged替代")]
    public static event System.Action OnNewWeek;
    [System.Obsolete("请使用OnYearChanged替代，OnYearChanged现在包含新年的语义")]
    public static event System.Action OnNewYear;
    #endregion
    
    #endregion
    
    #region 事件触发方法 - Event Trigger Methods
    
    #region 资源事件触发方法
    public static void TriggerResourceChanged(ResourceEventArgs args) 
    {
        OnResourceChanged?.Invoke(args);
    }
    #endregion
    
    #region 建筑事件触发方法
    public static void TriggerBuildingBuilt(BuildingEventArgs args) 
    {
        OnBuildingBuilt?.Invoke(args);
    }
    #endregion
    
    #region NPC事件触发方法
    public static void TriggerNPCStateChanged(NPCEventArgs args)
    {
        OnNPCStateChanged?.Invoke(args);
    }
    
    public static void TriggerNPCRelationshipChanged(NPCEventArgs args)
    {
        OnNPCRelationshipChanged?.Invoke(args);
    }
    
    public static void TriggerNPCSocialInteraction(NPCEventArgs args) 
    {
        OnNPCSocialInteraction?.Invoke(args);
    }
    
    public static void TriggerNPCSocialInteractionEnded(NPCEventArgs args)
    {
        OnNPCSocialInteractionEnded?.Invoke(args);
    }
    
    public static void TriggerNPCHired(NPCEventArgs args)
    {
        OnNPCHired?.Invoke(args);
    }
    
    public static void TriggerNPCFired(NPCEventArgs args)
    {
        OnNPCFired?.Invoke(args);
    }
    
    public static void TriggerNPCInstantiated(NPCEventArgs args)
    {
        OnNPCInstantiated?.Invoke(args);
    }
    
    public static void TriggerNPCDestroyed(NPCEventArgs args)
    {
        OnNPCDestroyed?.Invoke(args);
    }
    #endregion
    
    #region 时间事件触发方法
    public static void TriggerTimeChanged(GameTime newTime) 
    {
        OnTimeChanged?.Invoke(newTime);
    }
    
    public static void TriggerHourChanged(int hour) 
    {
        OnHourChanged?.Invoke(hour);
    }
    
    public static void TriggerDayChanged(int day) 
    {
        OnDayChanged?.Invoke(day);
        // 向后兼容
        OnDayStarted?.Invoke();
        OnDayPassed?.Invoke();
    }
    
    public static void TriggerMonthChanged(int month) 
    {
        OnMonthChanged?.Invoke(month);
    }
    
    public static void TriggerYearChanged(int year) 
    {
        OnYearChanged?.Invoke(year);
        // 向后兼容
        OnNewYear?.Invoke();
    }
    
    public static void TriggerSeasonChanged(Season season) 
    {
        OnSeasonChanged?.Invoke(season);
    }
    
    public static void TriggerWeekChanged(int week) 
    {
        OnWeekChanged?.Invoke(week);
        // 向后兼容
        OnNewWeek?.Invoke();
    }
    
    public static void TriggerNightStarted() 
    {
        OnNightStarted?.Invoke();
    }
    
    public static void TriggerWorkTimeStarted() 
    {
        OnWorkTimeStarted?.Invoke();
    }
    
    public static void TriggerWorkTimeEnded() 
    {
        OnWorkTimeEnded?.Invoke();
    }
    
    public static void TriggerRestTimeStarted() 
    {
        OnRestTimeStarted?.Invoke();
    }
    #endregion
    
    #region 已废弃的触发方法 (向后兼容)
    [System.Obsolete("请使用TriggerDayChanged替代")]
    public static void TriggerDayStarted() => TriggerDayChanged(TimeManager.Instance?.CurrentTime.day ?? 1);
    
    [System.Obsolete("请使用TriggerWeekChanged替代")]
    public static void TriggerNewWeek() => TriggerWeekChanged(GetCurrentWeek());
    
    [System.Obsolete("请使用TriggerYearChanged替代")]
    public static void TriggerNewYear() => TriggerYearChanged(TimeManager.Instance?.CurrentTime.year ?? 1);
    #endregion
    
    #endregion
    
    #region 辅助方法 - Helper Methods
    
    // 计算当前周数
    private static int GetCurrentWeek()
    {
        if (TimeManager.Instance?.CurrentTime != null)
        {
            var time = TimeManager.Instance.CurrentTime;
            return ((time.year - 1) * 12 * 30 + (time.month - 1) * 30 + time.day - 1) / 7 + 1;
        }
        return 1;
    }
    
    #endregion
    
    #region 使用示例和文档 - Usage Examples & Documentation
    
    /* 用法参考
    发布事件（系统内部）：
    public class ResourceManager : MonoBehaviour 
    {
        public bool AddResource(ResourceType type, int subType, int amount) 
        {
            int oldAmount = GetResourceAmount(type, subType);
            
            // 执行资源添加逻辑
            resources[type][subType] += amount;
            
            int newAmount = GetResourceAmount(type, subType);
            
            // 发布事件，通知其他系统
            var eventArgs = new ResourceEventArgs(type, subType, oldAmount, newAmount, "系统添加");
            GameEvents.TriggerResourceChanged(eventArgs);
            
            return true;
        }
    }

    监听事件（其他系统）：
    public class ReportManager : MonoBehaviour 
    {
        private void Start() 
        {
            // 订阅所有需要记录的事件
            GameEvents.OnResourceChanged += RecordResourceChange;
            GameEvents.OnBuildingBuilt += RecordBuildingEvent;
            GameEvents.OnNPCHired += RecordNPCEvent;
        }
        
        private void RecordResourceChange(ResourceEventArgs args) 
        {
            // 记录资源变化到报告系统
            dataCollector.RecordResourceChange(args);
        }
        
        private void RecordBuildingEvent(BuildingEventArgs args) 
        {
            // 记录建筑事件
            dataCollector.RecordBuildingEvent(args);
        }
    }
    
    public class UIManager : MonoBehaviour 
    {
        private void Start() 
        {
            // UI系统也可以监听这些事件来更新界面
            GameEvents.OnResourceChanged += UpdateResourceDisplay;
            GameEvents.OnBuildingBuilt += UpdateBuildingList;
        }
        
        private void UpdateResourceDisplay(ResourceEventArgs args) 
        {
            // 更新资源显示UI
            resourceUI.UpdateDisplay(args.resourceType, args.newAmount);
        }
    }

    事件流程示例：
    资源变化 → 通知UI更新、报告系统记录、检查解锁条件
    建筑建造 → 通知加成系统重新计算、报告系统记录、UI更新
    NPC状态变化 → 通知社交系统、工作效率重新计算、报告记录
    时间推进 → 触发工资支付、生产周期、NPC状态检查
    */
    
    #endregion
}
