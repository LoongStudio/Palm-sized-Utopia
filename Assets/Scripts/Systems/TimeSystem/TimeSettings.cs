using UnityEngine;

[CreateAssetMenu(fileName = "TimeSettings", menuName = "Utopia/Time Settings")]
public class TimeSettings : ScriptableObject
{
    [Header("时间流逝设置")]
    [SerializeField] private float timeScale = 1f;                    // 时间倍速
    [SerializeField] private float realSecondsPerGameHour = 60f;      // 现实多少秒等于游戏1小时
    [SerializeField] private bool pauseAtNight = false;               // 是否在夜间暂停
    [SerializeField] private int nightStartHour = 22;                 // 夜间开始时间
    [SerializeField] private int nightEndHour = 6;                    // 夜间结束时间
    
    [Header("游戏开始时间")]
    [SerializeField] private int startYear = 1;
    [SerializeField] private int startMonth = 3;                      // 从春季开始
    [SerializeField] private int startDay = 1;
    [SerializeField] private int startHour = 8;                       // 早上8点开始
    
    [Header("时间事件设置")]
    [SerializeField] private bool enableSeasonChange = true;          // 启用季节变化
    [SerializeField] private bool enableDayNightCycle = true;         // 启用昼夜循环
    [SerializeField] private bool enableYearlyEvents = true;          // 启用年度事件
    
    // 属性访问器
    public float TimeScale => timeScale;
    public float RealSecondsPerGameHour => realSecondsPerGameHour;
    public bool PauseAtNight => pauseAtNight;
    public int NightStartHour => nightStartHour;
    public int NightEndHour => nightEndHour;
    public GameTime StartTime => new GameTime(startYear, startMonth, startDay, startHour);
    public bool EnableSeasonChange => enableSeasonChange;
    public bool EnableDayNightCycle => enableDayNightCycle;
    public bool EnableYearlyEvents => enableYearlyEvents;
    
    // 验证配置
    private void OnValidate()
    {
        timeScale = Mathf.Max(0.1f, timeScale);
        realSecondsPerGameHour = Mathf.Max(1f, realSecondsPerGameHour);
        nightStartHour = Mathf.Clamp(nightStartHour, 0, 23);
        nightEndHour = Mathf.Clamp(nightEndHour, 0, 23);
        startMonth = Mathf.Clamp(startMonth, 1, 12);
        startDay = Mathf.Clamp(startDay, 1, 30);
        startHour = Mathf.Clamp(startHour, 0, 23);
    }
}
