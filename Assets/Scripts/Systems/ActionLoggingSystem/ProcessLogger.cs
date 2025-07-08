using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class ProcessLogger : MonoBehaviour
{
    private float timer = 0f;
    public float interval = 10f; // 每10秒统计一次

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            LogProcesses();
        }
    }

    void LogProcesses()
    {
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.ProcessName) && !IsSystemProcess(p));
        foreach (var proc in processes)
        {
            GameInstanceStats.Instance?.AddProcess(proc.ProcessName);
        }
    }

    bool IsSystemProcess(Process p)
    {
        string[] systemNames = { "System", "Idle", "svchost", "wininit", "csrss" };
        return systemNames.Contains(p.ProcessName);
    }
} 