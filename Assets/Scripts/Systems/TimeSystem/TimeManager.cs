using UnityEngine;
using System.Collections;

/// <summary>
/// 时间管理器 - 负责游戏时间的推进和管理
/// </summary>
public class TimeManager : SingletonManager<TimeManager>
{
    [Header("时间设置")]
    [SerializeField] private TimeSettings settings;
    
    [Header("当前时间")]
    [SerializeField] private GameTime currentTime;
    
    [Header("时间控制")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] private float currentTimeScale = 1f;
    
    // 私有字段
    private float timeAccumulator = 0f;
    private GameTime previousTime;
    private Coroutine timeUpdateCoroutine;
    
    // 属性
    public GameTime CurrentTime => currentTime;
    public Season CurrentSeason => currentTime.Season;
    public bool IsPaused => isPaused;
    public float TimeScale => currentTimeScale;
    public TimeSettings Settings => settings;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeTime();
    }
    
    private void Start()
    {
        StartTimeSystem();
    }
    
    private void InitializeTime()
    {
        if (settings != null)
        {
            currentTime = settings.StartTime;
            currentTimeScale = settings.TimeScale;
        }
        else
        {
            currentTime = new GameTime(1, 3, 1, 8); // 默认时间
        }
        
        previousTime = currentTime;
        Debug.Log($"[TimeManager] 时间系统初始化完成，当前时间: {currentTime.ToLongString()}");
    }
    
    private void StartTimeSystem()
    {
        if (timeUpdateCoroutine != null)
        {
            StopCoroutine(timeUpdateCoroutine);
        }
        
        timeUpdateCoroutine = StartCoroutine(TimeUpdateLoop());
        Debug.Log("[TimeManager] 时间系统启动");
    }
    
    private IEnumerator TimeUpdateLoop()
    {
        while (true)
        {
            if (!isPaused && !IsNightTimePaused())
            {
                UpdateTime();
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    private void UpdateTime()
    {
        if (settings == null) return;
        
        // 计算时间推进
        float deltaTime = Time.fixedDeltaTime * currentTimeScale;
        timeAccumulator += deltaTime;
        
        // 检查是否需要推进游戏时间
        float gameHourDuration = settings.RealSecondsPerGameHour;
        
        if (timeAccumulator >= gameHourDuration)
        {
            int hoursToAdd = Mathf.FloorToInt(timeAccumulator / gameHourDuration);
            timeAccumulator -= hoursToAdd * gameHourDuration;
            
            AdvanceTime(hoursToAdd);
        }
    }
    
    private void AdvanceTime(int hours)
    {
        previousTime = currentTime;
        currentTime = currentTime.AddHours(hours);
        
        // 触发相应的事件
        CheckAndTriggerEvents();
        
        // 触发基础时间变化事件
        TimeEvents.TriggerTimeChanged(currentTime);
    }
    
    private void CheckAndTriggerEvents()
    {
        // 检查小时变化
        if (currentTime.hour != previousTime.hour)
        {
            TimeEvents.TriggerHourChanged(currentTime.hour);
            CheckSpecialHourEvents();
        }
        
        // 检查天变化
        if (currentTime.day != previousTime.day)
        {
            TimeEvents.TriggerDayChanged(currentTime.day);
            TimeEvents.TriggerDayStarted();
            
            // 检查周变化（每7天）
            if (currentTime.day % 7 == 1)
            {
                TimeEvents.TriggerNewWeek();
            }
        }
        
        // 检查月变化
        if (currentTime.month != previousTime.month)
        {
            TimeEvents.TriggerMonthChanged(currentTime.month);
            
            // 检查季节变化
            if (currentTime.Season != previousTime.Season)
            {
                TimeEvents.TriggerSeasonChanged(currentTime.Season);
            }
        }
        
        // 检查年变化
        if (currentTime.year != previousTime.year)
        {
            TimeEvents.TriggerYearChanged(currentTime.year);
            TimeEvents.TriggerNewYear();
        }
    }
    
    private void CheckSpecialHourEvents()
    {
        if (settings == null) return;
        
        // 夜晚开始
        if (currentTime.hour == settings.NightStartHour)
        {
            TimeEvents.TriggerNightStarted();
            TimeEvents.TriggerWorkTimeEnded();
            TimeEvents.TriggerRestTimeStarted();
        }
        
        // 白天开始
        if (currentTime.hour == settings.NightEndHour)
        {
            TimeEvents.TriggerWorkTimeStarted();
        }
    }
    
    private bool IsNightTimePaused()
    {
        if (settings == null || !settings.PauseAtNight) return false;
        
        int nightStart = settings.NightStartHour;
        int nightEnd = settings.NightEndHour;
        
        if (nightStart < nightEnd)
        {
            return false; // 夜间不跨天，不暂停
        }
        else
        {
            // 夜间跨天的情况
            return currentTime.hour >= nightStart || currentTime.hour < nightEnd;
        }
    }
    
    // 公共方法
    public void PauseTime()
    {
        isPaused = true;
        Debug.Log("[TimeManager] 时间暂停");
    }
    
    public void ResumeTime()
    {
        isPaused = false;
        Debug.Log("[TimeManager] 时间恢复");
    }
    
    public void SetTimeScale(float scale)
    {
        currentTimeScale = Mathf.Max(0.1f, scale);
        Debug.Log($"[TimeManager] 时间倍速设置为: {currentTimeScale}x");
    }
    
    public void SetTime(GameTime newTime)
    {
        previousTime = currentTime;
        currentTime = newTime;
        CheckAndTriggerEvents();
        TimeEvents.TriggerTimeChanged(currentTime);
        Debug.Log($"[TimeManager] 时间设置为: {currentTime.ToLongString()}");
    }
    
    public bool IsWorkingHours()
    {
        if (settings == null) return true;
        
        int nightStart = settings.NightStartHour;
        int nightEnd = settings.NightEndHour;
        
        if (nightStart < nightEnd)
        {
            return true; // 夜间不跨天，全天工作
        }
        else
        {
            return currentTime.hour < nightStart && currentTime.hour >= nightEnd;
        }
    }
    
    public bool IsNightTime()
    {
        if (settings == null) return false;
        
        int nightStart = settings.NightStartHour;
        int nightEnd = settings.NightEndHour;
        
        if (nightStart < nightEnd)
        {
            return false; // 夜间不跨天
        }
        else
        {
            return currentTime.hour >= nightStart || currentTime.hour < nightEnd;
        }
    }
    
    // 调试方法
    [ContextMenu("推进1小时")]
    private void DebugAdvanceHour() => AdvanceTime(1);
    
    [ContextMenu("推进1天")]
    private void DebugAdvanceDay() => AdvanceTime(24);
    
    [ContextMenu("推进1月")]
    private void DebugAdvanceMonth() => AdvanceTime(24 * 30);
    
    [ContextMenu("打印当前时间")]
    private void DebugPrintTime() => Debug.Log($"当前时间: {currentTime.ToLongString()}");
}