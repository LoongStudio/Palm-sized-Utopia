using UnityEngine;

public class HardwareLogger : MonoBehaviour
{
    private float timer = 0f;
    public float interval = 5f; // 每5秒采集一次

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            float cpuTemp = GetCpuTemperature();
            GameInstanceStats.Instance?.UpdateCpuTemp(cpuTemp);
        }
    }

    // TODO: 替换为真实CPU温度采集
    float GetCpuTemperature()
    {
        // 这里只是模拟，实际可用OpenHardwareMonitor等库
        return Random.Range(40f, 80f);
    }
} 