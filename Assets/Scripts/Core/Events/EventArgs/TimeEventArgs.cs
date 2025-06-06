using UnityEngine;

/// <summary>
/// 时间事件参数类 - 使用对象初始化器模式
/// </summary>
public class TimeEventArgs 
{
    // 基础时间信息
    public GameTime currentTime { get; set; }
    public GameTime previousTime { get; set; }
    public TimeEventType eventType { get; set; }
    
    // 变化相关信息
    public int changedValue { get; set; }
    public Season currentSeason { get; set; }
    public Season previousSeason { get; set; }
    
    // 时间控制信息
    public float timeScale { get; set; } = 1f;
    public bool isPaused { get; set; } = false;
    
    // 元数据
    public string changeReason { get; set; } = "自然推进";
    public System.DateTime realTimestamp { get; set; }
    
    /// <summary>
    /// 时间事件类型枚举
    /// </summary>
    public enum TimeEventType 
    {
        TimeChanged,        // 时间变化（任何时间变化）
        HourChanged,        // 小时变化
        DayChanged,         // 天数变化
        MonthChanged,       // 月份变化
        YearChanged,        // 年份变化
        SeasonChanged,      // 季节变化
        WeekChanged,        // 周数变化
        NightStarted,       // 夜晚开始
        WorkTimeStarted,    // 工作时间开始
        WorkTimeEnded,      // 工作时间结束
        RestTimeStarted,    // 休息时间开始
        TimeScaleChanged,   // 时间倍速变化
        TimePaused,         // 时间暂停
        TimeResumed         // 时间恢复
    }
    
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public TimeEventArgs()
    {
        realTimestamp = System.DateTime.Now;
    }
    
    /// <summary>
    /// 自动计算相关属性（在设置基础属性后调用）
    /// </summary>
    public TimeEventArgs AutoCalculate()
    {
        // 自动设置季节信息
        if (currentTime != default(GameTime))
        {
            currentSeason = currentTime.Season;
        }
        
        if (previousTime != default(GameTime))
        {
            previousSeason = previousTime.Season;
        }
        
        // 根据事件类型自动计算变化数值
        if (currentTime != default(GameTime))
        {
            changedValue = eventType switch
            {
                TimeEventType.HourChanged => currentTime.hour,
                TimeEventType.DayChanged => currentTime.day,
                TimeEventType.MonthChanged => currentTime.month,
                TimeEventType.YearChanged => currentTime.year,
                TimeEventType.WeekChanged => TimeCalculationUtils.CalculateWeekNumber(currentTime),
                _ => changedValue // 保持已设置的值
            };
        }
        
        return this;
    }
    
    /// <summary>
    /// 静态工厂方法 - 创建基础时间变化事件
    /// </summary>
    public static TimeEventArgs CreateTimeChange(GameTime current, GameTime previous, TimeEventType type, string reason = "自然推进")
    {
        return new TimeEventArgs
        {
            currentTime = current,
            previousTime = previous,
            eventType = type,
            changeReason = reason
        }.AutoCalculate();
    }
    
    /// <summary>
    /// 静态工厂方法 - 创建时间节点事件
    /// </summary>
    public static TimeEventArgs CreateTimePoint(GameTime current, TimeEventType type)
    {
        return new TimeEventArgs
        {
            currentTime = current,
            previousTime = current,
            eventType = type,
            changeReason = type switch
            {
                TimeEventType.NightStarted => "夜晚开始",
                TimeEventType.WorkTimeStarted => "工作时间开始",
                TimeEventType.WorkTimeEnded => "工作时间结束",
                TimeEventType.RestTimeStarted => "休息时间开始",
                _ => "时间节点事件"
            }
        }.AutoCalculate();
    }
    
    /// <summary>
    /// 静态工厂方法 - 创建时间控制事件
    /// </summary>
    public static TimeEventArgs CreateTimeControl(GameTime current, TimeEventType type, float scale = 1f, bool paused = false)
    {
        return new TimeEventArgs
        {
            currentTime = current,
            previousTime = current,
            eventType = type,
            timeScale = scale,
            isPaused = paused,
            changeReason = type switch
            {
                TimeEventType.TimePaused => "时间暂停",
                TimeEventType.TimeResumed => "时间恢复",
                TimeEventType.TimeScaleChanged => "时间倍速变化",
                _ => "时间控制事件"
            }
        }.AutoCalculate();
    }
    
    // 注意：CalculateWeekNumber方法已移至TimeCalculationUtils.CalculateWeekNumber
    
    /// <summary>
    /// 获取变化描述
    /// </summary>
    public string GetChangeDescription()
    {
        return eventType switch
        {
            TimeEventType.TimeChanged => $"时间变化: {previousTime.ToShortString()} → {currentTime.ToShortString()}",
            TimeEventType.HourChanged => $"小时变化: {changedValue}:00",
            TimeEventType.DayChanged => $"新的一天: 第{changedValue}天",
            TimeEventType.MonthChanged => $"月份变化: 第{changedValue}月",
            TimeEventType.YearChanged => $"年份变化: 第{changedValue}年",
            TimeEventType.SeasonChanged => $"季节变化: {previousSeason.GetDisplayName()} → {currentSeason.GetDisplayName()}",
            TimeEventType.WeekChanged => $"周数变化: 第{changedValue}周",
            TimeEventType.NightStarted => "夜晚开始",
            TimeEventType.WorkTimeStarted => "工作时间开始",
            TimeEventType.WorkTimeEnded => "工作时间结束",
            TimeEventType.RestTimeStarted => "休息时间开始",
            TimeEventType.TimeScaleChanged => $"时间倍速变化: {timeScale}x",
            TimeEventType.TimePaused => "时间暂停",
            TimeEventType.TimeResumed => "时间恢复",
            _ => "未知时间事件"
        };
    }
    
    /// <summary>
    /// 是否是季节变化事件
    /// </summary>
    public bool IsSeasonChange => currentSeason != previousSeason;
    
    /// <summary>
    /// 获取时间差（小时）
    /// </summary>
    public long GetTimeDifferenceInHours()
    {
        return TimeCalculationUtils.GetHoursDifference(previousTime, currentTime);
    }
}

/* 使用示例 - 对象初始化器模式

// 1. 基础时间变化事件 - 使用对象初始化器
var timeChangeArgs = new TimeEventArgs
{
    currentTime = newTime,
    previousTime = oldTime,
    eventType = TimeEventArgs.TimeEventType.DayChanged,
    changeReason = "自然推进"
}.AutoCalculate();

// 2. 时间控制事件 - 灵活设置需要的属性
var pauseArgs = new TimeEventArgs
{
    currentTime = currentTime,
    eventType = TimeEventArgs.TimeEventType.TimePaused,
    isPaused = true,
    timeScale = 0f,
    changeReason = "玩家暂停"
}.AutoCalculate();

// 3. 使用静态工厂方法（简化版）
var dayChangeArgs = TimeEventArgs.CreateTimeChange(newTime, oldTime, TimeEventArgs.TimeEventType.DayChanged);
var nightStartArgs = TimeEventArgs.CreateTimePoint(currentTime, TimeEventArgs.TimeEventType.NightStarted);
var scaleChangeArgs = TimeEventArgs.CreateTimeControl(currentTime, TimeEventArgs.TimeEventType.TimeScaleChanged, 2.0f);

// 4. 自定义事件参数
var customArgs = new TimeEventArgs
{
    currentTime = currentTime,
    eventType = TimeEventArgs.TimeEventType.SeasonChanged,
    changeReason = "手动设置季节",
    changedValue = 1 // 可以手动设置特殊值
}.AutoCalculate();

优点：
- 清晰显示设置了哪些属性
- 灵活性高，只设置需要的属性
- 支持链式调用 .AutoCalculate()
- 扩展性好，添加新属性不影响现有代码
- 代码可读性强
*/
