using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 时间管理器 - 负责游戏时间的推进和管理
/// </summary>
public class TimeManager : SingletonManager<TimeManager>
{
    #region 字段和属性 (Fields & Properties)
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("时间设置")]
    [SerializeField] private TimeSettings settings;
    
    [Header("当前时间")]
    [SerializeField] private GameTime currentTime;
    
    [Header("时间控制")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] public float currentTimeScale = 1f;
    
    // 私有字段
    private float timeAccumulator = 0f;
    private GameTime previousTime;
    private Coroutine timeUpdateCoroutine;
    
    // 属性
    public GameTime CurrentTime => currentTime;
    [ShowInInspector] public Season CurrentSeason => currentTime.Season;
    public bool IsPaused => isPaused;
    public float TimeScale => currentTimeScale;
    public TimeSettings Settings => settings;
    
    // 新增：UI时间变动事件
    public delegate void TimeChangedDelegate(GameTime currentTime, GameTime previousTime);
    public event TimeChangedDelegate OnTimeChanged;
    
    #endregion
    
    #region Unity生命周期 (Unity Lifecycle)
    
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }
    
    private void Start()
    {
        StartTimeSystem();
    }
    
    #endregion
    
    #region 初始化 (Initialization)
    
    public void Initialize()
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
        if(showDebugInfo)
            Debug.Log($"[TimeManager] 时间系统初始化完成，当前时间: {currentTime.ToLongString()}");
    }
    
    private void StartTimeSystem()
    {
        if (timeUpdateCoroutine != null)
        {
            StopCoroutine(timeUpdateCoroutine);
        }
        
        timeUpdateCoroutine = StartCoroutine(TimeUpdateLoop());
        if(showDebugInfo)
            Debug.Log("[TimeManager] 时间系统启动");
    }
    
    #endregion
    
    #region 时间更新核心逻辑 (Time Update Core Logic)
    
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
        GameEvents.TriggerTimeChanged(currentTime, previousTime);
        // 新增：通知UI
        OnTimeChanged?.Invoke(currentTime, previousTime);
    }
    
    #endregion
    
    #region 事件处理 (Event Handling)
    
    private void CheckAndTriggerEvents()
    {
        // 检查小时变化
        if (currentTime.hour != previousTime.hour)
        {
            GameEvents.TriggerHourChanged(currentTime, previousTime);
            CheckSpecialHourEvents();
        }
        
        // 检查天变化
        if (currentTime.day != previousTime.day)
        {
            GameEvents.TriggerDayChanged(currentTime, previousTime);
            
            // 检查周变化（每7天）
            int currentWeek = TimeCalculationUtils.CalculateWeekNumber(currentTime);
            int previousWeek = TimeCalculationUtils.CalculateWeekNumber(previousTime);
            if (currentWeek != previousWeek)
            {
                GameEvents.TriggerWeekChanged(currentTime, previousTime);
            }
        }
        
        // 检查月变化
        if (currentTime.month != previousTime.month)
        {
            GameEvents.TriggerMonthChanged(currentTime, previousTime);
            
            // 检查季节变化
            if (currentTime.Season != previousTime.Season)
            {
                GameEvents.TriggerSeasonChanged(currentTime, previousTime);
            }
        }
        
        // 检查年变化
        if (currentTime.year != previousTime.year)
        {
            GameEvents.TriggerYearChanged(currentTime, previousTime);
        }
    }
    
    private void CheckSpecialHourEvents()
    {
        if (settings == null) return;
        
        // 夜晚开始
        if (currentTime.hour == settings.NightStartHour)
        {
            GameEvents.TriggerNightStarted(currentTime);
        }
        
        // 注意：工作时间相关的事件已移除，由其他系统负责判断和触发
        // 如需要工作状态变化，请在相应的工作管理系统中监听时间变化事件并自行判断
    }
    
    // 注意：GetWeekNumber方法已移至TimeCalculationUtils.CalculateWeekNumber
    
    #endregion
    
    #region 公共接口 (Public Interface)
    
    public void PauseTime()
    {
        isPaused = true;
        if(showDebugInfo)
            Debug.Log("[TimeManager] 时间暂停");
    }
    
    public void ResumeTime()
    {
        isPaused = false;
        if(showDebugInfo)
            Debug.Log("[TimeManager] 时间恢复");
    }
    
    public void SetTimeScale(float scale)
    {
        // TODO: 这里是否有点小问题
        currentTimeScale = Mathf.Max(0.1f, scale);
        // currentTimeScale = Mathf.Clamp(scale, 0f, 10f);
        if(showDebugInfo)
            Debug.Log($"[TimeManager] 时间倍率设置为: {currentTimeScale}");
    }
    
    public void SetTime(GameTime newTime)
    {
        previousTime = currentTime;
        currentTime = newTime;
        CheckAndTriggerEvents();
        GameEvents.TriggerTimeChanged(currentTime, previousTime, "手动设置");
        Debug.Log($"[TimeManager] 时间设置为: {currentTime.ToLongString()}");
        // 新增：通知UI
        OnTimeChanged?.Invoke(currentTime, previousTime);
    }
    
    #endregion
    
    #region 辅助方法 (Helper Methods)
    
    // 根据是否夜晚，返回是否暂停
    private bool IsNightTimePaused()
    {
        if (settings == null || !settings.PauseAtNight) return false;
        
        return TimeCalculationUtils.IsNightTime(currentTime, settings.NightStartHour, settings.NightEndHour);
    }

    // 是否是工作时间
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
    
    // 是否是夜晚
    public bool IsNightTime()
    {
        if (settings == null) return false;
        
        return TimeCalculationUtils.IsNightTime(currentTime, settings.NightStartHour, settings.NightEndHour);
    }
    
    #endregion
    
    #region 调试方法 (Debug Methods)
    
    [ContextMenu("推进1小时")]
    private void DebugAdvanceHour() => AdvanceTime(1);
    
    [ContextMenu("推进1天")]
    private void DebugAdvanceDay() => AdvanceTime(24);
    
    [ContextMenu("推进1月")]
    private void DebugAdvanceMonth() => AdvanceTime(24 * 30);
    
    [ContextMenu("打印当前时间")]
    private void DebugPrintTime() => Debug.Log($"当前时间: {currentTime.ToLongString()}");
    
    [ContextMenu("测试时间计算工具")]
    private void DebugTestTimeCalculationUtils()
    {
        Debug.Log("[TimeManager] 测试TimeCalculationUtils功能:");
        Debug.Log(TimeCalculationUtils.GetTimeCalculationDebugInfo(currentTime));
        
        bool isValid = TimeCalculationUtils.ValidateTimeCalculation(currentTime);
        Debug.Log($"时间计算验证结果: {(isValid ? "通过" : "失败")}");
        
        if (settings != null)
        {
            bool isNight = TimeCalculationUtils.IsNightTime(currentTime, settings.NightStartHour, settings.NightEndHour);
            Debug.Log($"当前是否为夜晚: {isNight}");
        }
    }
    
    #endregion
}