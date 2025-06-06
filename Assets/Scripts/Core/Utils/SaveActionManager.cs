using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class SaveActionManager
{
    private const string SaveFilePrefix = "action_log_";
    private const string SaveFileExt = ".dat";

    public static void SaveIncremental(List<InputEventSnapshot> snapshots)
    {
        if (snapshots == null || snapshots.Count == 0) return;

        string timestampKey = DateTime.Now.ToString("yyyyMMdd_HHmm");
        string filename = $"{SaveFilePrefix}{timestampKey}{SaveFileExt}";
        string path = SavePathUtil.GetSaveFilePath(filename);

        SaveActionData data = new SaveActionData(snapshots);
        string json = JsonUtility.ToJson(data);
        byte[] plain = Encoding.UTF8.GetBytes(json);
        byte[] encrypted = CryptoUtil.EncryptBytes(plain);

        File.WriteAllBytes(path, encrypted);
        Debug.Log($"[SaveActionManager] Incremental log saved: {filename}");
    }

    public static List<InputEventSnapshot> LoadRange(DateTime from, DateTime to)
    {
        string dir = SavePathUtil.GetSaveFilePath(""); // get directory
        if (!Directory.Exists(dir)) return new List<InputEventSnapshot>();

        List<InputEventSnapshot> result = new List<InputEventSnapshot>();

        foreach (string path in Directory.GetFiles(dir, $"{SaveFilePrefix}*{SaveFileExt}"))
        {
            string file = Path.GetFileNameWithoutExtension(path);
            if (!TryExtractTimestamp(file, out DateTime fileTime)) continue;

            if (fileTime >= from && fileTime <= to)
            {
                try
                {
                    byte[] encrypted = File.ReadAllBytes(path);
                    byte[] plain = CryptoUtil.DecryptBytes(encrypted);
                    string json = Encoding.UTF8.GetString(plain);
                    SaveActionData data = JsonUtility.FromJson<SaveActionData>(json);
                    result.AddRange(data.history);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LoadRange] Failed to load {file}: {e.Message}");
                }
            }
        }

        return result;
    }

    private static bool TryExtractTimestamp(string filename, out DateTime time)
    {
        // Expected: action_log_YYYYMMDD_HHMM
        time = default;
        if (!filename.StartsWith(SaveFilePrefix)) return false;

        string stamp = filename.Substring(SaveFilePrefix.Length);
        return DateTime.TryParseExact(stamp, "yyyyMMdd_HHmm", null, System.Globalization.DateTimeStyles.None, out time);
    }
}
