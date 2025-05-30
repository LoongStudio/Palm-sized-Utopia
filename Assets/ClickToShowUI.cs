using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WindowManager;
public class ClickToShowUI : MonoBehaviour, ISelectable
{
	private Camera mainCamera;
	private Canvas canvas;
	public GameObject nameTagPrefab;
	
	public GameObject currentUI;
	private bool showUI = false;
	private void Start()
	{
		if (mainCamera == null) mainCamera = Camera.main;
		if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
		
		// 初始化UI
		Destroy(currentUI);
		currentUI = Instantiate(nameTagPrefab, canvas.transform);
		currentUI.GetComponent<ContextUIAttributes>().belongTo = this.gameObject;
		currentUI.SetActive(showUI);
		//
	}
	
	public void OnSelected()
	{
		currentUI.SetActive(true);
	}

	public void OnDeselected()
	{
		currentUI.SetActive(false);
	}
	
	
}
