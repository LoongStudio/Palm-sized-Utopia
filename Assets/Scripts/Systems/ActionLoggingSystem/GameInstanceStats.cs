using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(
    typeof(HardwareLogger), 
    typeof(GlobalInputLogger), 
    typeof(ProcessLogger))
]
public class GameInstanceStats : SingletonManager<GameInstanceStats>
{
    // 1. 按键计数
    [ShowInInspector]
    private Dictionary<string, int> keyCount = new Dictionary<string, int>();

    // 2. 鼠标移动距离
    public float mouseDistance = 0f;
    public Vector2 lastMousePos;

    // 3. 后台进程计数
    public Dictionary<string, int> processCount = new Dictionary<string, int>();

    // 4. 当前分钟CPU最高温度
    public float currentMinuteMaxCpuTemp = 0f;
    public float lastCpuTempCheckTime = 0f;
    
    // 1. 记录按键
    public void AddKey(string key)
    {
        if (!keyCount.ContainsKey(key))
            keyCount[key] = 0;
        keyCount[key]++;
    }

    // 2. 记录鼠标移动
    public void AddMouseMove(Vector2 newPos)
    {
        if (lastMousePos != Vector2.zero)
            mouseDistance += Vector2.Distance(lastMousePos, newPos);
        lastMousePos = newPos;
    }

    // 3. 记录进程
    public void AddProcess(string processName)
    {
        if (!processCount.ContainsKey(processName))
            processCount[processName] = 0;
        processCount[processName]++;
    }

    // 4. 记录CPU温度
    public void UpdateCpuTemp(float temp)
    {
        if (Time.time - lastCpuTempCheckTime > 60f)
        {
            // 新的一分钟，重置
            currentMinuteMaxCpuTemp = temp;
            lastCpuTempCheckTime = Time.time;
        }
        else
        {
            if (temp > currentMinuteMaxCpuTemp)
                currentMinuteMaxCpuTemp = temp;
        }
    }

    // 实时获取接口
    public Dictionary<string, int> GetKeyCounts() => keyCount;
    public float GetMouseDistance() => mouseDistance;
    public Dictionary<string, int> GetProcessCounts() => processCount;
    public float GetCurrentMinuteMaxCpuTemp() => currentMinuteMaxCpuTemp;
} 