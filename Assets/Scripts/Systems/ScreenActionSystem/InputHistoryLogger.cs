using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GlobalInputListener))]
public class InputHistoryLogger : SingletonManager<InputHistoryLogger>
{
	[SerializeField] private List<InputEventSnapshot> history = new List<InputEventSnapshot>();
	private InputEventSnapshot _currentSnapshot;

	private Vector2 _lastMousePosition;
	// 增量保存计时器
	private float _autoSaveTimer = 0f;
	private float _timer = 0f;
	private const float AutoSaveInterval = 60f;
	private void Start()
	{
		_currentSnapshot = new InputEventSnapshot();
	}
	private void Update()
	{
		// 模拟鼠标移动距离更新（用于前台调试，后台由钩子处理）
		Vector2 current = Input.mousePosition;
		float delta = Vector2.Distance(current, _lastMousePosition);
		_currentSnapshot.mouseMoveDistance += delta;
		_lastMousePosition = current;
		// 增量保存
		_timer += Time.deltaTime;
		_autoSaveTimer += Time.deltaTime;

		if (_timer >= 0.5f)
		{
			CommitSnapshot();
			_timer = 0;
		}

		if (_autoSaveTimer >= AutoSaveInterval)
		{
			SaveActionManager.SaveIncremental(history);
			ClearHistory(); // 清除已存储部分
			_autoSaveTimer = 0;
		}
	}

	public void LogKey(string keyDesc)
	{
		_currentSnapshot.keyEvents.Add(keyDesc);
	}

	public void LogMouse(string mouseDesc)
	{
		_currentSnapshot.mouseEvents.Add(mouseDesc);
	}
	
	public void AddMouseMoveDistance(float dist)
	{
		_currentSnapshot.mouseMoveDistance += dist;
	}


	public void CommitSnapshot()
	{
		if (_currentSnapshot.keyEvents.Count > 0 || _currentSnapshot.mouseEvents.Count > 0 || _currentSnapshot.mouseMoveDistance > 0)
		{
			history.Add(_currentSnapshot);
			_currentSnapshot = new InputEventSnapshot();
		}
	}

	public List<InputEventSnapshot> GetHistory()
	{
		return history;
	}

	public void ClearHistory()
	{
		history.Clear();
	}
}
