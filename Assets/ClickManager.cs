using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ClickManager : MonoBehaviour
{
	public static ClickManager Instance { get; private set; }
	private EventSystem eventSystem;
	
	public GameObject selectedObject;
	public List<GameObject> openedWindowObjects = new List<GameObject>();
	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void Start()
	{
		eventSystem = EventSystem.current;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			// 如果当前有物体被选择，如果点击的不是物体和UI 则 关闭UI
			//					如果是 则 不变
			// 如果当前没有物体被选择，如果点击的不是物体和UI 则 不变
			//					如果点击的是可触发物体 则 打开UI
			// 当我放屁
			// bool hitSelected = false;
			GameObject hitTarget = null;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				hitTarget = hit.collider.gameObject;
			}
			if (hitTarget != null 
			    && !FindFirstObjectByType<EditorManager>().inEditMode			// 不在编辑模式中
			    && hitTarget.TryGetComponent(out ISelectable availableTarget)
			    && !openedWindowObjects.Contains(hitTarget))
			{
				Debug.Log("创建新的窗口： " + hitTarget.name);
				availableTarget.OnSelected();
				selectedObject = hitTarget;
				openedWindowObjects.Add(hitTarget);
				// 点击物体不能让其到最上层，会出现点击穿透 然后浮出来的情况
				// selectedObject.GetComponent<ClickToShowUI>().currentUI.transform.SetAsLastSibling();
			}
		}

		CheckAndCloseOverlappingUI(openedWindowObjects);
	}
	
	public void CheckAndCloseOverlappingUI(List<GameObject> openedWindowObjects)
	{
		// 创建一个副本列表，避免在遍历中直接修改原列表
		List<GameObject> toRemove = new List<GameObject>();

		for (int i = 0; i < openedWindowObjects.Count; i++)
		{
			GameObject objA = openedWindowObjects[i];
			var uiA = objA.GetComponent<ClickToShowUI>().currentUI;
			if (uiA == null || !uiA.activeSelf) continue;

			RectTransform rectA = uiA.GetComponent<RectTransform>();

			for (int j = i + 1; j < openedWindowObjects.Count; j++)
			{
				GameObject objB = openedWindowObjects[j];
				var uiB = objB.GetComponent<ClickToShowUI>().currentUI;
				if (uiB == null || !uiB.activeSelf) continue;

				RectTransform rectB = uiB.GetComponent<RectTransform>();

				if (IsOverlapping(rectA, rectB))
				{
					// 比较层级，关闭下面的并标记移除
					if (openedWindowObjects.IndexOf(objA) < openedWindowObjects.IndexOf(objB))
					{
						CloseWindow(objA);
					}
					else
					{
						CloseWindow(objB);
					}
				}
			}
		}

		// 从列表中移除已关闭的 UI 对象
		foreach (var obj in toRemove)
		{
			// Debug.Log("移除相关窗口： " + obj.name);
			openedWindowObjects.Remove(obj);
		}
	}

	
	private bool IsOverlapping(RectTransform rectA, RectTransform rectB)
	{
		Vector3[] cornersA = new Vector3[4];
		Vector3[] cornersB = new Vector3[4];

		rectA.GetWorldCorners(cornersA);
		rectB.GetWorldCorners(cornersB);

		Rect aRect = new Rect(cornersA[0], cornersA[2] - cornersA[0]);
		Rect bRect = new Rect(cornersB[0], cornersB[2] - cornersB[0]);

		return aRect.Overlaps(bRect);
	}

	public void CloseWindow(GameObject objBelongTo)
	{
		objBelongTo.GetComponent<ClickToShowUI>().currentUI.SetActive(false);
		openedWindowObjects.Remove(objBelongTo);
		if (selectedObject == objBelongTo)
			selectedObject = null;
	}
	public void CloseAllWindows()
	{
		Debug.Log("清空所有窗口");
		foreach (var obj in openedWindowObjects)
		{
			obj.GetComponent<ClickToShowUI>().currentUI.SetActive(false);
		}
		openedWindowObjects.Clear();
		selectedObject = null;
	}
	public bool GetPointerTouchUI(GameObject targetUI)
	{
		GraphicRaycaster raycaster = targetUI.GetComponent<GraphicRaycaster>();
		PointerEventData pointerData = new PointerEventData(eventSystem)
		{
			position = Input.mousePosition
		};

		List<RaycastResult> results = new List<RaycastResult>();
		raycaster.Raycast(pointerData, results);
		// cast 到的第一个内容
		if (results.Count == 0) return false;
		// foreach (RaycastResult result in results)
		// {
		// 	if (result.gameObject == targetUI 
		// 	    || result.gameObject.transform.IsChildOf(targetUI.transform))
		// 		return true;
		// }
		if (results[0].gameObject == targetUI 
		    || results[0].gameObject.transform.IsChildOf(targetUI.transform))
			return true;
		return false;
	}
}
