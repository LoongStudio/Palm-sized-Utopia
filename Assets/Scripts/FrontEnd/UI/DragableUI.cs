using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;

public class DragableUI : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [InfoBox("带有这个组件的UI可以被拖动，但不会影响其他UI的交互", InfoMessageType.Info)]
    [LabelText("目标面板")]
    [Tooltip("拖动的目标面板，即跟随鼠标移动的面板")]
    public RectTransform targetPanel;
    private Vector2 offset;
    private Canvas canvas;

    private bool allowDrag = true;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 如果点击的是 Button/InputField 等交互组件，就不允许拖动
        if (IsPointerOverUIElement(eventData))
        {
            allowDrag = false;
            return;
        }

        allowDrag = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPanel, eventData.position, eventData.pressEventCamera, out offset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!allowDrag) return;

        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPos))
        {
            targetPanel.anchoredPosition = localPos - offset;
        }
    }

    private bool IsPointerOverUIElement(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var go = result.gameObject;
            if (
                go.GetComponent<Button>() ||
                go.GetComponent<Toggle>() ||
                go.GetComponent<InputField>() ||
                go.GetComponent<Scrollbar>() ||
                go.GetComponent<Slider>() ||
                go.GetComponent<TMP_InputField>())
            {
                return true;
            }
        }

        return false;
    }
    
}