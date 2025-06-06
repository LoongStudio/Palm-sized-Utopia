using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using GlobalInputPlugin;
using System.Runtime.Serialization.Formatters.Binary;
public class InputHistoryLogger : MonoBehaviour
{
    public float snapshotInterval = 1f;
    private float timeAccumulator = 0f;

    private InputEventSnapshot currentSnapshot;
    private Vector2 lastMousePos = Vector2.zero;

    [SerializeField] private readonly ConcurrentQueue<InputEventSnapshot> snapshotQueue = new();
    // private string savePath;
    void Awake()
    {
        Application.runInBackground = true;
    }

    void OnEnable()
    {
        // savePath = Path.Combine(Application.persistentDataPath, "input_history.log");
        currentSnapshot = new InputEventSnapshot();
        GlobalInputListener.OnInputEvent += OnInputEvent;
        GlobalInputListener.Init();

        // 启动异步保存任务
        Task.Run(SnapshotWriterLoop);
    }

    void OnDisable()
    {
        GlobalInputListener.OnInputEvent -= OnInputEvent;
        GlobalInputListener.Stop();
    }

    void Update()
    {
        timeAccumulator += Time.deltaTime;
        if (timeAccumulator >= snapshotInterval)
        {
            timeAccumulator = 0f;
            snapshotQueue.Enqueue(currentSnapshot);
            currentSnapshot = new InputEventSnapshot();
        }
    }

    private void OnInputEvent(InputEventData e)
    {
        switch (e.EventType)
        {
            case InputEventData.InputType.KeyDown:
                currentSnapshot.keyEvents.Add($"Down:{e.KeyCode}");
                break;
            case InputEventData.InputType.KeyUp:
                currentSnapshot.keyEvents.Add($"Up:{e.KeyCode}");
                break;
            case InputEventData.InputType.MouseClick:
                currentSnapshot.mouseEvents.Add($"Click:{e.MouseButton}");
                break;
            case InputEventData.InputType.MouseMove:
                float dist = Vector2.Distance(new Vector2(e.MouseX, e.MouseY), lastMousePos);
                currentSnapshot.mouseMoveDistance += dist;
                lastMousePos = new Vector2(e.MouseX, e.MouseY);
                break;
            case InputEventData.InputType.MouseWheel:
                currentSnapshot.mouseEvents.Add($"Wheel:{e.WheelDelta}");
                break;
        }
    }

    private readonly TimeSpan saveInterval = TimeSpan.FromMinutes(5);
    private DateTime lastSaveTime = DateTime.MinValue;

    private async Task SnapshotWriterLoop()
    {
        List<InputEventSnapshot> buffer = new();

        while (true)
        {
            // 先把队列里所有数据都取出来，累积到 buffer
            while (snapshotQueue.TryDequeue(out var snapshot))
            {
                buffer.Add(snapshot);
            }

            // 判断是否到保存时间 && buffer中有数据
            if (buffer.Count > 0 && (DateTime.Now - lastSaveTime) >= saveInterval)
            {
                try
                {
                    string filename = $"input_{DateTime.Now:yyyyMMdd_HHmmss}.bytes";
                    string fullPath = SavePathUtil.GetSaveFilePath(filename);

                    using MemoryStream memStream = new();
                    BinaryFormatter formatter = new();
                    formatter.Serialize(memStream, buffer); // 直接序列化 List<InputEventSnapshot>
                    byte[] raw = memStream.ToArray();

                    // 加密
                    byte[] encrypted = CryptoUtil.EncryptBytes(raw);

                    // 保存为二进制
                    await File.WriteAllBytesAsync(fullPath, encrypted);

                    buffer.Clear();
                    lastSaveTime = DateTime.Now;  // 更新保存时间
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Snapshot save error: {ex.Message}");
                }
            }

            // 等待短时间再继续循环（避免CPU空转）
            await Task.Delay(1000);
        }
    }


    public async Task<List<InputEventSnapshot>> LoadSnapshotsInRange(DateTime start, DateTime end)
    {
        var dir = Path.GetDirectoryName(SavePathUtil.GetSaveFilePath("dummy"));
        var files = Directory.GetFiles(dir, "input_*.bytes");

        List<InputEventSnapshot> result = new();

        foreach (var file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (DateTime.TryParseExact(name.Substring(6), "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.None, out var ts))
            {
                if (ts >= start && ts <= end)
                {
                    try
                    {
                        byte[] encrypted = await File.ReadAllBytesAsync(file);
                        byte[] decrypted = CryptoUtil.DecryptBytes(encrypted);

                        using MemoryStream memStream = new(decrypted);
                        BinaryFormatter formatter = new();
                        // return (List<InputEventSnapshot>)formatter.Deserialize(memStream);
                        var snapshotList = (List<InputEventSnapshot>)formatter.Deserialize(memStream);
                        result.AddRange(snapshotList);

                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to load snapshot from {file}: {ex.Message}");
                    }
                }
            }
        }

        return result;
    }
    
    [Serializable]
    public class SnapshotBatch
    {
        public List<InputEventSnapshot> snapshots;

        public SnapshotBatch(List<InputEventSnapshot> list)
        {
            snapshots = list;
        }
    }

}
