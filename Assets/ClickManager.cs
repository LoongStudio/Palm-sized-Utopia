using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WindowManager;

public class ClickManager : MonoBehaviour
{
	public static ClickManager Instance { get; private set; }
	private EventSystem eventSystem;
	
	public GameObject selectedObject;

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
			bool hitSelected = false;
			GameObject hitTarget = null;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				hitTarget = hit.collider.gameObject;
				if (hit.transform.gameObject == selectedObject) hitSelected = true;
			}
			// if (hit != null && hit.transform.TryGetComponent(out ClickToShowUI clickToShowUI))
			// {
			// 	if (GetPointerTouchUI(clickToShowUI.currentUI)) hitSelected = true;
			// }
			if (selectedObject == null
				&& hitTarget != null 
			    && hitTarget.TryGetComponent(out ISelectable availableTarget))
			{
				availableTarget.OnSelected();
				selectedObject = hitTarget;
				// 点击物体不能让其到最上层，会出现点击穿透 然后浮出来的情况
				// selectedObject.GetComponent<ClickToShowUI>().currentUI.transform.SetAsLastSibling();
			}
			
		}
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
