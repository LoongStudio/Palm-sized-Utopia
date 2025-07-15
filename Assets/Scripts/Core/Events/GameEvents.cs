using UnityEngine;

public static class GameEvents 
{
    #region 事件定义 - Event Definitions
    
    #region 资源相关事件
    public static event System.Action<ResourceEventArgs> OnResourceChanged;
    public static event System.Action<ResourceEventArgs> OnResourceInsufficient;
    public static event System.Action<ResourceEventArgs> OnResourceBoughtClicked;
    public static event System.Action<ResourceEventArgs> OnResourceBoughtConfirmed;

    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action<ResourceEventArgs> OnResourceAdded;
    // public static event System.Action<ResourceEventArgs> OnResourceRemoved;
    #endregion

    #region 建筑相关事件
    public static event System.Action<BuildingEventArgs> OnLandBought;
    public static event System.Action<BuildingEventArgs> OnBuildingBought;
    public static event System.Action<BuildingEventArgs> OnBuildingBuilt;
    public static event System.Action<BuildingEventArgs> OnBuildingPlaced;
    public static event System.Action<BuildingEventArgs> OnBoughtBuildingPlacedAfterDragging;
    public static event System.Action<BuildingEventArgs> OnBuildingCreated;
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action<BuildingEventArgs> OnBuildingUpgraded;
    // public static event System.Action<BuildingEventArgs> OnBuildingDestroyed;
    #endregion

    #region NPC相关事件
    public static event System.Action<NPCEventArgs> OnNPCHired;
    public static event System.Action<NPCEventArgs> OnNPCInstantiated;
    public static event System.Action<NPCEventArgs> OnNPCLoadedFromData;
    public static event System.Action<NPCEventArgs> OnNPCCreatedFromList;
    public static event System.Action<NPCEventArgs> OnNPCFired;
    public static event System.Action<NPCEventArgs> OnNPCDestroyed;
    public static event System.Action<NPCEventArgs> OnNPCStateChanged;
    public static event System.Action<NPCEventArgs> OnNPCRelationshipChanged;
    public static event System.Action<NPCEventArgs> OnNPCShouldStartSocialInteraction;
    public static event System.Action<NPCEventArgs> OnNPCInSocialPosition;
    public static event System.Action<NPCEventArgs> OnNPCReadyForSocialInteraction;
    public static event System.Action<NPCEventArgs> OnNPCSocialInteractionStarted;
    public static event System.Action<NPCEventArgs> OnNPCSocialInteractionEnded;
    #endregion
    
    #region 时间相关事件
    // 核心时间事件 (使用TimeEventArgs统一参数)
    public static event System.Action<TimeEventArgs> OnTimeChanged;           // 任何时间变化（细粒度）
    public static event System.Action<TimeEventArgs> OnHourChanged;           // 小时变化
    public static event System.Action<TimeEventArgs> OnDayChanged;            // 天数变化（包含新一天开始的语义）
    public static event System.Action<TimeEventArgs> OnMonthChanged;          // 月份变化
    public static event System.Action<TimeEventArgs> OnYearChanged;           // 年份变化（包含新年的语义）
    public static event System.Action<TimeEventArgs> OnSeasonChanged;         // 季节变化
    public static event System.Action<TimeEventArgs> OnWeekChanged;           // 周数变化（每7天）
    
    // 特殊时间节点事件
    public static event System.Action<TimeEventArgs> OnNightStarted;          // 夜晚开始
    public static event System.Action<TimeEventArgs> OnWorkTimeStarted;       // 工作时间开始
    public static event System.Action<TimeEventArgs> OnWorkTimeEnded;         // 工作时间结束
    public static event System.Action<TimeEventArgs> OnRestTimeStarted;       // 休息时间开始
    public static event System.Action<TimeEventArgs> OnTimeScaleChanged;      // 时间倍速变化
    public static event System.Action<TimeEventArgs> OnTimePaused;            // 时间暂停
    public static event System.Action<TimeEventArgs> OnTimeResumed;           // 时间恢复
    #endregion

    #region 游戏流程事件
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public static event System.Action OnGameStarted;
    // public static event System.Action OnGamePaused;
    public static event System.Action<bool> OnEditModeChanged;
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
    public static void TriggerResourceInsufficient(ResourceEventArgs args) 
    {
        OnResourceInsufficient?.Invoke(args);
    }
    public static void TriggerResourceBoughtClicked(ResourceEventArgs args) 
    {
        OnResourceBoughtClicked?.Invoke(args);
    }
    public static void TriggerResourceBoughtConfirmed(ResourceEventArgs args) 
    {
        OnResourceBoughtConfirmed?.Invoke(args);
    }
    #endregion
    
    #region 建筑事件触发方法
    public static void TriggerLandBought(BuildingEventArgs args) 
    {
        OnLandBought?.Invoke(args);
    }
    public static void TriggerBuildingBought(BuildingEventArgs args) 
    {
        OnBuildingBought?.Invoke(args);
    }
    public static void TriggerBuildingBuilt(BuildingEventArgs args) 
    {
        OnBuildingBuilt?.Invoke(args);
    }
    public static void TriggerBuildingPlaced(BuildingEventArgs args) 
    {
        Debug.Log($"[GameEvents] TriggerBuildingPlaced called with event type: {args.eventType}, building: {args.building?.name}, timestamp: {args.timestamp}");
        OnBuildingPlaced?.Invoke(args);
    }
    public static void TriggerBoughtBuildingPlacedAfterDragging(BuildingEventArgs args) 
    {
        OnBoughtBuildingPlacedAfterDragging?.Invoke(args);
    }
    public static void TriggerBuildingCreated(BuildingEventArgs args) 
    {
        OnBuildingCreated?.Invoke(args);
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

    public static void TriggerNPCShouldStartSocialInteraction(NPCEventArgs args)
    {
        OnNPCShouldStartSocialInteraction?.Invoke(args);
    }

    public static void TriggerNPCInSocialPosition(NPCEventArgs args)
    {
        OnNPCInSocialPosition?.Invoke(args);
    }

    public static void TriggerNPCReadyForSocialInteraction(NPCEventArgs args)
    {
        OnNPCReadyForSocialInteraction?.Invoke(args);
    }
    
    public static void TriggerNPCSocialInteractionStarted(NPCEventArgs args) 
    {
        OnNPCSocialInteractionStarted?.Invoke(args);
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

    public static void TriggerNPCCreatedFromList(NPCEventArgs args)
    {
        OnNPCCreatedFromList?.Invoke(args);
    }

    public static void TriggerNPCLoadedFromData(NPCEventArgs args)
    {
        OnNPCLoadedFromData?.Invoke(args);
    }
    
    public static void TriggerNPCDestroyed(NPCEventArgs args)
    {
        OnNPCDestroyed?.Invoke(args);
    }
    #endregion
    
    #region 时间事件触发方法
    public static void TriggerTimeChanged(GameTime currentTime, GameTime previousTime, string changeReason = "自然推进") 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = previousTime,
            eventType = TimeEventArgs.TimeEventType.TimeChanged,
            changeReason = changeReason
        }.AutoCalculate();
        OnTimeChanged?.Invoke(args);
    }
    
    public static void TriggerHourChanged(GameTime currentTime, GameTime previousTime, string changeReason = "自然推进") 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = previousTime,
            eventType = TimeEventArgs.TimeEventType.HourChanged,
            changeReason = changeReason
        }.AutoCalculate();
        OnHourChanged?.Invoke(args);
    }
    
    public static void TriggerDayChanged(GameTime currentTime, GameTime previousTime, string changeReason = "自然推进") 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = previousTime,
            eventType = TimeEventArgs.TimeEventType.DayChanged,
            changeReason = changeReason
        }.AutoCalculate();
        OnDayChanged?.Invoke(args);
        // 向后兼容
        OnDayStarted?.Invoke();
        OnDayPassed?.Invoke();
    }
    
    public static void TriggerMonthChanged(GameTime currentTime, GameTime previousTime, string changeReason = "自然推进") 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = previousTime,
            eventType = TimeEventArgs.TimeEventType.MonthChanged,
            changeReason = changeReason
        }.AutoCalculate();
        OnMonthChanged?.Invoke(args);
    }
    
    public static void TriggerYearChanged(GameTime currentTime, GameTime previousTime, string changeReason = "自然推进") 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = previousTime,
            eventType = TimeEventArgs.TimeEventType.YearChanged,
            changeReason = changeReason
        }.AutoCalculate();
        OnYearChanged?.Invoke(args);
        // 向后兼容
        OnNewYear?.Invoke();
    }
    
    public static void TriggerSeasonChanged(GameTime currentTime, GameTime previousTime, string changeReason = "自然推进") 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = previousTime,
            eventType = TimeEventArgs.TimeEventType.SeasonChanged,
            changeReason = changeReason
        }.AutoCalculate();
        OnSeasonChanged?.Invoke(args);
    }
    
    public static void TriggerWeekChanged(GameTime currentTime, GameTime previousTime, string changeReason = "自然推进") 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = previousTime,
            eventType = TimeEventArgs.TimeEventType.WeekChanged,
            changeReason = changeReason
        }.AutoCalculate();
        OnWeekChanged?.Invoke(args);
        // 向后兼容
        OnNewWeek?.Invoke();
    }
    
    public static void TriggerNightStarted(GameTime currentTime) 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = currentTime,
            eventType = TimeEventArgs.TimeEventType.NightStarted,
            changeReason = "夜晚开始"
        }.AutoCalculate();
        OnNightStarted?.Invoke(args);
    }
    
    public static void TriggerWorkTimeStarted(GameTime currentTime) 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = currentTime,
            eventType = TimeEventArgs.TimeEventType.WorkTimeStarted,
            changeReason = "工作时间开始"
        }.AutoCalculate();
        OnWorkTimeStarted?.Invoke(args);
    }
    
    public static void TriggerWorkTimeEnded(GameTime currentTime) 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = currentTime,
            eventType = TimeEventArgs.TimeEventType.WorkTimeEnded,
            changeReason = "工作时间结束"
        }.AutoCalculate();
        OnWorkTimeEnded?.Invoke(args);
    }
    
    public static void TriggerRestTimeStarted(GameTime currentTime) 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = currentTime,
            eventType = TimeEventArgs.TimeEventType.RestTimeStarted,
            changeReason = "休息时间开始"
        }.AutoCalculate();
        OnRestTimeStarted?.Invoke(args);
    }
    
    public static void TriggerTimeScaleChanged(GameTime currentTime, float newTimeScale) 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = currentTime,
            eventType = TimeEventArgs.TimeEventType.TimeScaleChanged,
            timeScale = newTimeScale,
            changeReason = "时间倍速变化"
        }.AutoCalculate();
        OnTimeScaleChanged?.Invoke(args);
    }
    
    public static void TriggerTimePaused(GameTime currentTime) 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = currentTime,
            eventType = TimeEventArgs.TimeEventType.TimePaused,
            timeScale = 0f,
            isPaused = true,
            changeReason = "时间暂停"
        }.AutoCalculate();
        OnTimePaused?.Invoke(args);
    }
    
    public static void TriggerTimeResumed(GameTime currentTime, float timeScale) 
    {
        var args = new TimeEventArgs
        {
            currentTime = currentTime,
            previousTime = currentTime,
            eventType = TimeEventArgs.TimeEventType.TimeResumed,
            timeScale = timeScale,
            isPaused = false,
            changeReason = "时间恢复"
        }.AutoCalculate();
        OnTimeResumed?.Invoke(args);
    }
    #endregion

    #region 游戏流程事件触发方法
    public static void TriggerEditModeChanged(bool isEditMode)
    {
        OnEditModeChanged?.Invoke(isEditMode);
        Debug.Log($"[GameEvents] Edit Mode Changed: {isEditMode}");
    }
    #endregion
    
    #region 已废弃的触发方法 (向后兼容)
    [System.Obsolete("请使用TriggerDayChanged替代")]
    public static void TriggerDayStarted() 
    {
        if (TimeManager.Instance?.CurrentTime != null)
            TriggerDayChanged(TimeManager.Instance.CurrentTime, TimeManager.Instance.CurrentTime, "向后兼容调用");
    }
    
    [System.Obsolete("请使用TriggerWeekChanged替代")]
    public static void TriggerNewWeek() 
    {
        if (TimeManager.Instance?.CurrentTime != null)
            TriggerWeekChanged(TimeManager.Instance.CurrentTime, TimeManager.Instance.CurrentTime, "向后兼容调用");
    }
    
    [System.Obsolete("请使用TriggerYearChanged替代")]
    public static void TriggerNewYear() 
    {
        if (TimeManager.Instance?.CurrentTime != null)
            TriggerYearChanged(TimeManager.Instance.CurrentTime, TimeManager.Instance.CurrentTime, "向后兼容调用");
    }
    #endregion
    
    #endregion
    
    #region 辅助方法 - Helper Methods
    
    // 计算当前周数
    private static int GetCurrentWeek()
    {
        if (TimeManager.Instance?.CurrentTime != null)
        {
            return TimeCalculationUtils.CalculateWeekNumber(TimeManager.Instance.CurrentTime);
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
