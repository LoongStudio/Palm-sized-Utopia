using System;
using UnityEngine;

/// <summary>
/// 时间系统事件
/// </summary>
public static class TimeEvents
{
    // 基础时间事件
    public static event Action<GameTime> OnTimeChanged;              // 时间变化
    public static event Action<int> OnHourChanged;                   // 小时变化
    public static event Action<int> OnDayChanged;                    // 天数变化
    public static event Action<int> OnMonthChanged;                  // 月份变化
    public static event Action<int> OnYearChanged;                   // 年份变化
    public static event Action<Season> OnSeasonChanged;              // 季节变化
    
    // 特殊时间事件
    public static event Action OnDayStarted;                        // 新的一天开始
    public static event Action OnNightStarted;                      // 夜晚开始
    public static event Action OnNewWeek;                           // 新的一周（每7天）
    public static event Action OnNewYear;                           // 新年
    
    // 工作时间事件
    public static event Action OnWorkTimeStarted;                   // 工作时间开始
    public static event Action OnWorkTimeEnded;                     // 工作时间结束
    public static event Action OnRestTimeStarted;                   // 休息时间开始
    
    // 事件触发方法
    public static void TriggerTimeChanged(GameTime newTime) => OnTimeChanged?.Invoke(newTime);
    public static void TriggerHourChanged(int hour) => OnHourChanged?.Invoke(hour);
    public static void TriggerDayChanged(int day) => OnDayChanged?.Invoke(day);
    public static void TriggerMonthChanged(int month) => OnMonthChanged?.Invoke(month);
    public static void TriggerYearChanged(int year) => OnYearChanged?.Invoke(year);
    public static void TriggerSeasonChanged(Season season) => OnSeasonChanged?.Invoke(season);
    public static void TriggerDayStarted() => OnDayStarted?.Invoke();
    public static void TriggerNightStarted() => OnNightStarted?.Invoke();
    public static void TriggerNewWeek() => OnNewWeek?.Invoke();
    public static void TriggerNewYear() => OnNewYear?.Invoke();
    public static void TriggerWorkTimeStarted() => OnWorkTimeStarted?.Invoke();
    public static void TriggerWorkTimeEnded() => OnWorkTimeEnded?.Invoke();
    public static void TriggerRestTimeStarted() => OnRestTimeStarted?.Invoke();
}
