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
        var processes = System.Diagnostics.Process.GetProcesses();
        foreach (var proc in processes)
        {
            try
            {
                // 这里访问属性要用try-catch包裹
                if (!string.IsNullOrEmpty(proc.ProcessName) && !IsSystemProcess(proc))
                {
                    GameInstanceStats.Instance?.AddProcess(proc.ProcessName);
                }
            }
            catch (System.Exception)
            {
                // 进程已退出或无权限，忽略即可
            }
        }
    }

    bool IsSystemProcess(Process p)
    {
        string[] systemNames = { "System", "Idle", "svchost", "wininit", "csrss" };
        return systemNames.Contains(p.ProcessName);
    }
} 