using UnityEngine;

/// <summary>
/// 季节枚举
/// </summary>
public enum Season
{
    Spring = 0,  // 春季 (3,4,5月)
    Summer = 1,  // 夏季 (6,7,8月)
    Autumn = 2,  // 秋季 (9,10,11月)
    Winter = 3   // 冬季 (12,1,2月)
}

/// <summary>
/// 季节扩展方法
/// </summary>
public static class SeasonExtensions
{
    public static string GetDisplayName(this Season season)
    {
        return season switch
        {
            Season.Spring => "春季",
            Season.Summer => "夏季",
            Season.Autumn => "秋季",
            Season.Winter => "冬季",
            _ => "未知"
        };
    }
    
    public static Color GetSeasonColor(this Season season)
    {
        return season switch
        {
            Season.Spring => new Color(0.5f, 1f, 0.5f),      // 浅绿色
            Season.Summer => new Color(1f, 1f, 0.3f),        // 黄色
            Season.Autumn => new Color(1f, 0.6f, 0.2f),      // 橙色
            Season.Winter => new Color(0.7f, 0.9f, 1f),      // 浅蓝色
            _ => Color.white
        };
    }
}