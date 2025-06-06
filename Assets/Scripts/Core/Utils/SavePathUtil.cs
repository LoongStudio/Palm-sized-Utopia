using System.IO;
using UnityEngine;

public static class SavePathUtil
{
	public static string SavesFolderPath
	{
		get
		{
			// Application.dataPath 在不同平台代表不同含义：
			// - 编辑器下：Assets 文件夹路径
			// - Windows 构建后：xxx/xxx/YourGame_Data 文件夹路径
			// 所以用 Application.dataPath 的父目录代表可执行文件的目录。

			string exeFolder = Directory.GetParent(Application.dataPath).FullName;
			string savesPath = Path.Combine(exeFolder, "Saves");

			// 确保目录存在
			if (!Directory.Exists(savesPath))
			{
				Directory.CreateDirectory(savesPath);
			}

			return savesPath;
		}
	}

	public static string GetSaveFilePath(string fileName)
	{
		return Path.Combine(SavesFolderPath, fileName);
	}
}
