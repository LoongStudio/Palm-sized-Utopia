using System.Text;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
public class GridManager : MonoBehaviour
{
	public static GridManager Instance;
	public Dictionary<Vector3Int, BlockProperties> occupiedMap = new Dictionary<Vector3Int, BlockProperties>();

	void Awake()
	{
		Instance = this;
	}

	
	public bool IsOccupied(Vector3Int gridPos)
	{
		return occupiedMap.ContainsKey(gridPos);
	}
	public void RemoveOccupied(Vector3Int gridPos)
	{
		occupiedMap.Remove(gridPos);
	}
	public void SetOccupied(Vector3Int gridPos, BlockProperties state)
	{
		occupiedMap[gridPos] = state;
	}
	
	// 新增：打印所有占用格子
	[ContextMenu("Print Occupied Cells")]
	public void PrintOccupiedCells()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("Occupied grid cells:");

		foreach (var kvp in occupiedMap)
		{
			if (kvp.Value)
				sb.AppendLine(kvp.Key.ToString());
		}

		Debug.Log(sb.ToString());
	}
}











#if UNITY_EDITOR
public class GridOccupiedWindow : EditorWindow
{
	Vector2 scrollPos;

	[MenuItem("DEBUG/Grid Occupied Cells")]
	public static void ShowWindow()
	{
		GetWindow<GridOccupiedWindow>("Grid Occupied Cells");
	}

	void OnGUI()
	{
		GUILayout.Label("Occupied Grid Cells", EditorStyles.boldLabel);

		if (GridManager.Instance == null)
		{
			EditorGUILayout.HelpBox("No GridManager found in the scene.", MessageType.Warning);
			return;
		}

		var occupiedMap = GridManager.Instance.occupiedMap;
		List<Vector3Int> occupiedCells = new List<Vector3Int>();

		foreach (var kvp in occupiedMap)
		{
			if (kvp.Value)
				occupiedCells.Add(kvp.Key);
		}

		if (occupiedCells.Count == 0)
		{
			EditorGUILayout.LabelField("No occupied cells.");
			return;
		}

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		foreach (var cell in occupiedCells)
		{
			EditorGUILayout.LabelField(cell + " " + occupiedMap[cell].blockName);
		}

		EditorGUILayout.EndScrollView();
	}
}
#endif