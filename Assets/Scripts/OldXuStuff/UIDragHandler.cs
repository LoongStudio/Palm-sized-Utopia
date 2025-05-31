using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragTitleBar : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private RectTransform windowTransform;
	private Canvas canvas;

	public RectTransform targetWindow; // 拖动目标：整个窗口

	private void Awake()
	{
		if (targetWindow == null)
		{
			targetWindow = GetComponentInParent<Canvas>()?.GetComponentInChildren<RectTransform>();
			Debug.LogWarning("targetWindow 未设置，默认查找父窗口");
		}

		canvas = GetComponentInParent<Canvas>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		transform.SetAsLastSibling(); // 保证在最上层
		targetWindow.gameObject.GetComponent<ContextUIAttributes>().stackToBelong = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (targetWindow != null && canvas != null)
		{
			targetWindow.anchoredPosition += eventData.delta / canvas.scaleFactor;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		// 可加动画、边界检测等
	}
}
