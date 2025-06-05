/// <summary>
/// 时间格式化工具
/// </summary>
public static class TimeFormatter
{
    public static string FormatTime(GameTime time, TimeFormatType formatType)
    {
        return formatType switch
        {
            TimeFormatType.Short => $"{time.month}/{time.day} {time.hour:D2}:00",
            TimeFormatType.Medium => $"{time.year}年{time.month}月{time.day}日 {time.hour}:00",
            TimeFormatType.Long => $"{time.year}年{time.month}月{time.day}日 {time.hour}:00 {time.Season.GetDisplayName()}",
            TimeFormatType.DateOnly => $"{time.year}/{time.month:D2}/{time.day:D2}",
            TimeFormatType.TimeOnly => $"{time.hour:D2}:00",
            TimeFormatType.SeasonOnly => time.Season.GetDisplayName(),
            _ => time.ToString()
        };
    }
    
    public static string FormatDuration(int hours)
    {
        if (hours < 24)
            return $"{hours}小时";
        
        int days = hours / 24;
        int remainingHours = hours % 24;
        
        if (remainingHours == 0)
            return $"{days}天";
        
        return $"{days}天{remainingHours}小时";
    }
}

public enum TimeFormatType
{
    Short,      // 3/15 08:00
    Medium,     // 1年3月15日 8:00
    Long,       // 1年3月15日 8:00 春季
    DateOnly,   // 1/03/15
    TimeOnly,   // 08:00
    SeasonOnly  // 春季
}