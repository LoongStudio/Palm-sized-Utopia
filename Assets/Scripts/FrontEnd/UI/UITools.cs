using UnityEngine;
using UnityEngine.Localization.Settings;

public static class UITools
{
	public static string GetLocalizedText(string key, string tableName)
	{
		// 边界检查
		if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(tableName))
			return key;

		// 获取当前语言的字符串表格
		var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName);
		if (stringTable == null)
		{
			Debug.LogWarning($"本地化表格 {tableName} 不存在，使用原始文本");
			return key;
		}
		return stringTable.GetEntry(key)?.Value ?? key;
	}
}
