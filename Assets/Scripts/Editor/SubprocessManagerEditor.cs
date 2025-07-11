using UnityEditor;
using UnityEngine;

// 指定该编辑器脚本对应SubprocessManager组件
[CustomEditor(typeof(SubprocessManager))]
public class SubprocessManagerEditor : Editor
{
	private SubprocessManager targetScript;

	void OnEnable()
	{
		// 获取当前选中的SubprocessManager组件实例
		targetScript = (SubprocessManager)target;
	}

	// 重写Inspector绘制逻辑
	public override void OnInspectorGUI()
	{
		// 绘制默认的组件属性（保留原有字段显示）
		DrawDefaultInspector();

		// 添加一个分隔线
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("操作控制", EditorStyles.boldLabel);

		// 添加自定义按钮：触发FetchData()
		if (GUILayout.Button("立即获取数据"))
		{
			// 检查组件是否有效
			if (targetScript != null)
			{
				targetScript.FetchDataDirectly();
			}
		}
	}
}
