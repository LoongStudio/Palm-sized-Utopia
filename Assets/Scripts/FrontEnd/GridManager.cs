using System.Text;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
public class GridManager : SingletonManager<GridManager>
{
	protected override void Awake()
	{
		base.Awake();
	}
	private Dictionary<Vector2Int, BlockProperties> _occupiedMap = new Dictionary<Vector2Int, BlockProperties>();
	
	
	public bool IsOccupied(Vector2Int gridPos)
	{
		return _occupiedMap.ContainsKey(gridPos);
	}
	public void RemoveOccupied(Vector2Int gridPos)
	{
		_occupiedMap.Remove(gridPos);
	}
	public void 2(Vector2Int gridPos, BlockProperties state)
	{
		_occupiedMap[gridPos] = state;
	}
	
	// 新增：打印所有占用格子
	[ContextMenu("Print Occupied Cells")]
	public void PrintOccupiedCells()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("Occupied grid cells:");

		foreach (var kvp in _occupiedMap)
		{
			if (kvp.Value)
				sb.AppendLine(kvp.Key.ToString());
		}

		Debug.Log(sb.ToString());
	}
	
	public Dictionary<Vector2Int, BlockProperties> GetMap() 
	{
		return _occupiedMap;
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

		var occupiedMap = GridManager.Instance.GetMap();
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