using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 时间UI显示，自动监听TimeManager的时间变动
/// </summary>
public class TimeUI : MonoBehaviour
{
    public TimeManager timeManager; // 可在Inspector拖拽
    public Text timeText;           // 可在Inspector拖拽

    private void OnEnable()
    {
        if (timeManager == null)
            timeManager = TimeManager.Instance;
        if (timeManager != null)
            timeManager.OnTimeChanged += UpdateTimeUI;
    }

    private void OnDisable()
    {
        if (timeManager != null)
            timeManager.OnTimeChanged -= UpdateTimeUI;
    }

    private void Start()
    {
        // 初始化显示
        if (timeManager != null)
            UpdateTimeUI(timeManager.CurrentTime, timeManager.CurrentTime);
    }

    private void UpdateTimeUI(GameTime current, GameTime previous)
    {
        if (timeText != null)
            timeText.text = current.ToShortString();
    }
} 