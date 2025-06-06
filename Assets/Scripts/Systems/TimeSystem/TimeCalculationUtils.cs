using UnityEngine;

/// <summary>
/// 时间计算工具类 - 提供统一的时间相关计算方法
/// </summary>
public static class TimeCalculationUtils
{
    #region 周数计算
    
    /// <summary>
    /// 计算游戏时间对应的周数
    /// 规则：从游戏开始计算总天数，每7天为一周
    /// </summary>
    /// <param name="gameTime">游戏时间</param>
    /// <returns>周数（从1开始）</returns>
    public static int CalculateWeekNumber(GameTime gameTime)
    {
        if (gameTime == default(GameTime))
            return 1;
            
        // 计算从游戏开始的总天数
        int totalDays = GetTotalDaysFromGameStart(gameTime);
        
        // 每7天为一周，周数从1开始
        return totalDays / 7 + 1;
    }
    
    /// <summary>
    /// 计算从游戏开始到指定时间的总天数
    /// </summary>
    /// <param name="gameTime">目标游戏时间</param>
    /// <returns>总天数（从0开始）</returns>
    public static int GetTotalDaysFromGameStart(GameTime gameTime)
    {
        if (gameTime == default(GameTime))
            return 0;
            
        // 假设游戏从第1年第1月第1天开始
        // 每月30天，每年12个月
        int totalDays = (gameTime.year - 1) * 12 * 30 + 
                       (gameTime.month - 1) * 30 + 
                       (gameTime.day - 1);
                       
        return Mathf.Max(0, totalDays);
    }
    
    #endregion
    
    #region 时间差计算
    
    /// <summary>
    /// 计算两个游戏时间之间的小时差
    /// </summary>
    /// <param name="fromTime">起始时间</param>
    /// <param name="toTime">结束时间</param>
    /// <returns>小时差</returns>
    public static long GetHoursDifference(GameTime fromTime, GameTime toTime)
    {
        if (fromTime == default(GameTime) || toTime == default(GameTime))
            return 0;
            
        return toTime.TotalHours - fromTime.TotalHours;
    }
    
    /// <summary>
    /// 计算两个游戏时间之间的天数差
    /// </summary>
    /// <param name="fromTime">起始时间</param>
    /// <param name="toTime">结束时间</param>
    /// <returns>天数差</returns>
    public static int GetDaysDifference(GameTime fromTime, GameTime toTime)
    {
        if (fromTime == default(GameTime) || toTime == default(GameTime))
            return 0;
            
        int fromTotalDays = GetTotalDaysFromGameStart(fromTime);
        int toTotalDays = GetTotalDaysFromGameStart(toTime);
        
        return toTotalDays - fromTotalDays;
    }
    
    /// <summary>
    /// 计算两个游戏时间之间的周数差
    /// </summary>
    /// <param name="fromTime">起始时间</param>
    /// <param name="toTime">结束时间</param>
    /// <returns>周数差</returns>
    public static int GetWeeksDifference(GameTime fromTime, GameTime toTime)
    {
        int fromWeek = CalculateWeekNumber(fromTime);
        int toWeek = CalculateWeekNumber(toTime);
        
        return toWeek - fromWeek;
    }
    
    #endregion
    
    #region 特殊时间判断
    
    /// <summary>
    /// 判断指定时间是否为夜晚
    /// </summary>
    /// <param name="gameTime">游戏时间</param>
    /// <param name="nightStartHour">夜晚开始小时</param>
    /// <param name="nightEndHour">夜晚结束小时</param>
    /// <returns>是否为夜晚</returns>
    public static bool IsNightTime(GameTime gameTime, int nightStartHour, int nightEndHour)
    {
        if (nightStartHour < nightEndHour)
        {
            // 夜间不跨天的情况（如 22:00 - 23:59）
            return gameTime.hour >= nightStartHour && gameTime.hour < nightEndHour;
        }
        else
        {
            // 夜间跨天的情况（如 22:00 - 06:00）
            return gameTime.hour >= nightStartHour || gameTime.hour < nightEndHour;
        }
    }
    
    /// <summary>
    /// 判断是否是新的一天开始（小时为0）
    /// </summary>
    /// <param name="gameTime">游戏时间</param>
    /// <returns>是否是新的一天开始</returns>
    public static bool IsDayStart(GameTime gameTime)
    {
        return gameTime.hour == 0;
    }
    
    /// <summary>
    /// 判断是否是新的一周开始（周一，即第7天的倍数+1天）
    /// </summary>
    /// <param name="gameTime">游戏时间</param>
    /// <returns>是否是新的一周开始</returns>
    public static bool IsWeekStart(GameTime gameTime)
    {
        int totalDays = GetTotalDaysFromGameStart(gameTime);
        return totalDays % 7 == 0 && IsDayStart(gameTime);
    }
    
    #endregion
    
    #region 调试和验证方法
    
    /// <summary>
    /// 获取时间计算的调试信息
    /// </summary>
    /// <param name="gameTime">游戏时间</param>
    /// <returns>调试信息字符串</returns>
    public static string GetTimeCalculationDebugInfo(GameTime gameTime)
    {
        if (gameTime == default(GameTime))
            return "无效时间";
            
        int totalDays = GetTotalDaysFromGameStart(gameTime);
        int weekNumber = CalculateWeekNumber(gameTime);
        
        return $"时间: {gameTime.ToShortString()}\n" +
               $"总天数: {totalDays}\n" +
               $"周数: 第{weekNumber}周\n" +
               $"本周第{(totalDays % 7) + 1}天";
    }
    
    /// <summary>
    /// 验证时间计算的一致性
    /// </summary>
    /// <param name="gameTime">要验证的游戏时间</param>
    /// <returns>验证是否通过</returns>
    public static bool ValidateTimeCalculation(GameTime gameTime)
    {
        if (gameTime == default(GameTime))
            return false;
            
        // 验证总天数不能为负数
        int totalDays = GetTotalDaysFromGameStart(gameTime);
        if (totalDays < 0)
            return false;
            
        // 验证周数计算的合理性
        int weekNumber = CalculateWeekNumber(gameTime);
        if (weekNumber < 1)
            return false;
            
        // 验证周数和总天数的关系
        int expectedWeek = totalDays / 7 + 1;
        if (weekNumber != expectedWeek)
            return false;
            
        return true;
    }
    
    #endregion
} 