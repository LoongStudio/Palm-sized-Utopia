using System;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Net;
using Debug = UnityEngine.Debug;

public class SubprocessManager : SingletonManager<SubprocessManager>
{
    private string exeFolder;
    private string exePath;
    private Process subprocess;
    public bool isSubprocessRunning = false;

    void Start()
    {
        // 获取游戏安装目录的根目录
        exeFolder = Directory.GetParent(Application.dataPath).FullName;
        // 提取exe文件到安装目录
        StartCoroutine(ExtractExeFromResources());
        // #if UNITY_EDITOR 
        // 启动子进程
        StartCoroutine(StartSubprocessWithRetry());
        // // 定期从API获取数据
        // StartCoroutine(FetchDataPeriodically());
    }

    IEnumerator ExtractExeFromResources()
    {
        // 构建目标文件路径（确保文件名与后续使用一致）
        exePath = Path.Combine(exeFolder, "ActionMonitor.exe");
    
        // 检查目标文件是否已存在且有效
        if (File.Exists(exePath))
        {
            try
            {
                // 验证文件是否完整（可添加文件大小或哈希检查）
                FileInfo fileInfo = new FileInfo(exePath);
                if (fileInfo.Length > 1024) // 简单验证：文件大小大于1KB
                {
                    UnityEngine.Debug.Log($"[SubProcess] 使用现有exe文件: {exePath}");
                    yield break; // 无需重新提取，直接返回
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[SubProcess] 现有exe文件可能损坏，准备重新提取");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[SubProcess] 检查现有exe文件失败: {ex.Message}");
                // 继续执行提取流程
            }
        }

        // 构建资源路径
        string resourcePath = "file://" + Application.dataPath + "/Resources/SubProcess/ActionMonitor-v0.1.3.exe";
    
        using (UnityWebRequest request = UnityWebRequest.Get(resourcePath))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] exeData = request.downloadHandler.data;
            
                // 确保目标目录存在
                Directory.CreateDirectory(exeFolder);
            
                // 写入文件
                File.WriteAllBytes(exePath, exeData);
            
                UnityEngine.Debug.Log($"[SubProcess] 已从原始资源加载exe文件: {exePath}");
            }
            else
            {
                UnityEngine.Debug.LogError($"[SubProcess] 加载exe文件失败: {request.error}");
            }
        }
    }

    IEnumerator StartSubprocessWithRetry(int maxRetries = 3)
    {
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            if (StartSubprocess())
            {
                // 等待子进程启动并检查API是否响应
                yield return new WaitForSeconds(2f);
                
                if (IsApiResponding())
                {
                    isSubprocessRunning = true;
                    yield break;
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[SubProcess] API未响应，尝试重启子进程 ({retryCount + 1}/{maxRetries})");
                    StopSubprocess();
                }
            }
            
            retryCount++;
            yield return new WaitForSeconds(1f);
        }
        
        UnityEngine.Debug.LogError("[SubProcess] 无法启动子进程或API无响应");
    }

    bool StartSubprocess()
    {
        try
        {
            if (File.Exists(exePath))
            {
                // 获取当前Unity进程的PID
                int unityPid = Process.GetCurrentProcess().Id;
            
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = unityPid.ToString(), // 传递PID作为参数
                    WorkingDirectory = exeFolder,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                subprocess = new Process { StartInfo = startInfo };
                subprocess.Start();
            
                // 监听子进程输出
                subprocess.OutputDataReceived += (sender, args) => {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        UnityEngine.Debug.Log($"[SubProcess] 子进程输出: {args.Data}");
                    }
                };
            
                subprocess.BeginOutputReadLine();
            
                // 监听错误输出
                subprocess.ErrorDataReceived += (sender, args) => {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        UnityEngine.Debug.LogError($"[SubProcess] 子进程错误: {args.Data}");
                    }
                };
            
                subprocess.BeginErrorReadLine();
            
                UnityEngine.Debug.Log($"[SubProcess] 子进程已启动，传递Unity PID: {unityPid}");
                return true;
            }
            else
            {
                UnityEngine.Debug.LogError($"[SubProcess] exe文件不存在: {exePath}");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"[SubProcess] 启动子进程失败: {ex.Message}");
            return false;
        }
    }

    void StopSubprocess()
    {
        try
        {
            if (subprocess != null && !subprocess.HasExited)
            {
                subprocess.Kill();
                subprocess.WaitForExit(1000);
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"[SubProcess] 停止子进程失败: {ex.Message}");
        }
        finally
        {
            subprocess?.Dispose();
            subprocess = null;
            isSubprocessRunning = false;
        }
    }

    bool IsApiResponding()
    {
        try
        {
            using (var client = new WebClient())
            {
                client.DownloadString("http://localhost:18080/ping");
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    IEnumerator FetchDataPeriodically()
    {
        // 等待子进程启动
        yield return new WaitUntil(() => isSubprocessRunning);

        while (isSubprocessRunning)
        {
            yield return StartCoroutine(FetchData());
            yield return new WaitForSeconds(1f); // 每秒请求一次
        }
    }

    public void FetchDataDirectly() => StartCoroutine(FetchData());
    
    IEnumerator FetchData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get("http://localhost:18080/current"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonData = request.downloadHandler.text;
                ProcessJsonData(jsonData);
            }
            else
            {
                UnityEngine.Debug.LogError($"[SubProcess] API请求失败: {request.error}");
                
                // 检查子进程是否还在运行
                if (subprocess == null || subprocess.HasExited)
                {
                    isSubprocessRunning = false;
                    UnityEngine.Debug.LogWarning("[SubProcess] 子进程已退出，尝试重启");
                    StartCoroutine(StartSubprocessWithRetry());
                }
            }
        }
    }

    void ProcessJsonData(string json)
    {
        try
        {
            Debug.Log($"[SubProcess] 获取平文格式： {json}");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"[SubProcess] 解析JSON失败: {ex.Message}\n数据: {json}");
        }
    }

    void OnDestroy()
    {
        StopSubprocess();
    }
}