using System;
using UnityEngine;

/// <summary>
/// 游戏时间数据结构
/// </summary>
[System.Serializable]
public struct GameTime : IEquatable<GameTime>
{
    [Header("时间信息")]
    public int year;        // 年份
    public int month;       // 月份 (1-12)
    public int day;         // 天数 (1-30)
    public int hour;        // 小时 (0-23)
    
    // 构造函数
    public GameTime(int year = 1, int month = 1, int day = 1, int hour = 0)
    {
        this.year = Mathf.Max(1, year);
        this.month = Mathf.Clamp(month, 1, 12);
        this.day = Mathf.Clamp(day, 1, 30);  // 一月固定30天
        this.hour = Mathf.Clamp(hour, 0, 23);
    }
    
    // 季节属性
    public Season Season
    {
        get
        {
            return month switch
            {
                3 or 4 or 5 => Season.Spring,
                6 or 7 or 8 => Season.Summer,
                9 or 10 or 11 => Season.Autumn,
                12 or 1 or 2 => Season.Winter,
                _ => Season.Spring
            };
        }
    }
    
    // 总小时数（用于比较和计算）
    public long TotalHours => 
        ((long)year - 1) * 12 * 30 * 24 + 
        (month - 1) * 30 * 24 + 
        (day - 1) * 24 + 
        hour;
    
    // 时间推进方法
    public GameTime AddHours(int hours)
    {
        var newTime = this;
        newTime.hour += hours;
        
        while (newTime.hour >= 24)
        {
            newTime.hour -= 24;
            newTime = newTime.AddDays(1);
        }
        
        while (newTime.hour < 0)
        {
            newTime.hour += 24;
            newTime = newTime.AddDays(-1);
        }
        
        return newTime;
    }
    
    public GameTime AddDays(int days)
    {
        var newTime = this;
        newTime.day += days;
        
        while (newTime.day > 30)
        {
            newTime.day -= 30;
            newTime = newTime.AddMonths(1);
        }
        
        while (newTime.day < 1)
        {
            newTime.day += 30;
            newTime = newTime.AddMonths(-1);
        }
        
        return newTime;
    }
    
    public GameTime AddMonths(int months)
    {
        var newTime = this;
        newTime.month += months;
        
        while (newTime.month > 12)
        {
            newTime.month -= 12;
            newTime.year++;
        }
        
        while (newTime.month < 1)
        {
            newTime.month += 12;
            newTime.year--;
            if (newTime.year < 1) newTime.year = 1;
        }
        
        return newTime;
    }
    
    // 格式化输出
    public string ToShortString() => $"{year}/{month:D2}/{day:D2} {hour:D2}:00";
    public string ToLongString() => $"{year}年{month}月{day}日 {hour}:00 ({Season})";
    
    // 相等性比较
    public bool Equals(GameTime other) => 
        year == other.year && month == other.month && day == other.day && hour == other.hour;
    
    public override bool Equals(object obj) => obj is GameTime other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(year, month, day, hour);
    public override string ToString() => ToShortString();
    
    // 操作符重载
    public static bool operator ==(GameTime left, GameTime right) => left.Equals(right);
    public static bool operator !=(GameTime left, GameTime right) => !left.Equals(right);
    public static bool operator <(GameTime left, GameTime right) => left.TotalHours < right.TotalHours;
    public static bool operator >(GameTime left, GameTime right) => left.TotalHours > right.TotalHours;
    public static bool operator <=(GameTime left, GameTime right) => left.TotalHours <= right.TotalHours;
    public static bool operator >=(GameTime left, GameTime right) => left.TotalHours >= right.TotalHours;
}