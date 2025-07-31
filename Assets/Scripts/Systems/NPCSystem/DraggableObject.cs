using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ComponentDisableEntry
{
    public Behaviour component;
    public bool disableDuringDrag = true;
}

public class DraggableObject : MonoBehaviour
{
    [Header("拖动设置")]
    [SerializeField] private float dragPlaneHeight = 0f; // 拖动平面的Y轴高度
    [SerializeField] private bool useFixedDragPlane = true; // 是否使用固定高度平面
    
    [Header("拖动期间禁用的组件")]
    [SerializeField] private List<ComponentDisableEntry> componentsToDisable = new List<ComponentDisableEntry>();
    [SerializeField] public NPC npc;
    [SerializeField] public Outline outline;
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float originalY;
    private bool isMouseOver = false;
    private float dragPlaneDistance; // 拖动平面距离相机的距离
    
    private void Start()
    {
        mainCamera = Camera.main;
        originalY = transform.position.y;
        outline.enabled = false;

        if (useFixedDragPlane)
        {
            // 计算拖动平面到相机的距离
            dragPlaneDistance = Mathf.Abs(dragPlaneHeight - mainCamera.transform.position.y);
        }
        else
        {
            // 使用物体当前的Y轴位置作为拖动平面
            dragPlaneHeight = originalY;
            dragPlaneDistance = Vector3.Dot(transform.position - mainCamera.transform.position, mainCamera.transform.forward);
        }
    }
    
    private void OnMouseEnter()
    {
        isMouseOver = true;
        outline.enabled = isMouseOver;
    }
    
    private void OnMouseExit()
    {
        isMouseOver = false;
        if (!isDragging) outline.enabled = isMouseOver;
    }
    
    private void Update()
    {
        // 检测右键点击
        if (isMouseOver && Input.GetMouseButtonDown(1)) // 1 代表右键
        {
            StartDrag();
        }
        
        // 检测右键释放
        if (isDragging && Input.GetMouseButtonUp(1))
        {
            EndDrag();
        }
        
        // 处理拖动逻辑
        if (isDragging)
        {
            HandleDrag();
        }
    }
    
    private void StartDrag()
    {
        if (mainCamera == null) return;
        
        isDragging = true;
        
        // 计算鼠标点击位置与物体位置的偏移
        Vector3 mousePosition = GetMouseWorldPositionOnDragPlane();
        offset = transform.position - mousePosition;

        npc.stateMachine.ChangeState(NPCState.Dragging);

        // 禁用指定的组件
        foreach (var entry in componentsToDisable)
        {
            if (entry.component != null && entry.disableDuringDrag)
            {
                entry.component.enabled = false;
            }
        }
    }
    
    private void HandleDrag()
    {
        if (!isDragging || mainCamera == null) return;
        
        Vector3 mousePosition = GetMouseWorldPositionOnDragPlane();
        Vector3 targetPosition = mousePosition + offset;
        
        // 保持Y轴在拖动平面上
        targetPosition.y = dragPlaneHeight;
        
        transform.position = targetPosition;
    }
    
    private void EndDrag()
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        // 重新启用指定的组件
        foreach (var entry in componentsToDisable)
        {
            if (entry.component != null && entry.disableDuringDrag)
            {
                entry.component.enabled = true;
            }
        }

        npc.stateMachine.ChangeState(NPCState.Idle);

        if (!isMouseOver) outline.enabled = false;
    }
    
    private Vector3 GetMouseWorldPositionOnDragPlane()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // 创建一个在指定高度的平面
        Plane plane = new Plane(Vector3.up, new Vector3(0, dragPlaneHeight, 0));
        
        // 计算射线与平面的交点
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        // 如果没有交点，返回默认位置
        return mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragPlaneDistance));
    }
}