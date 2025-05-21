using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class DragAndSnapWithAnchors : MonoBehaviour
{
    public List<Transform> bottomAnchors;  // ⬅️ 多个锚点
    public List<Vector3Int> regPositions { get; private set; } = new List<Vector3Int>();
    
    public float targetY = 0f;             // 吸附高度
    public bool autoSnapOnStart = true;
    private Vector3 startPosition;
    private bool isDragging = false;
    private Camera cam;
    private Plane groundPlane;
    // DEBUG    
    public bool debugDrawAnchors = true;
    public Color anchorColor = Color.yellow;
    public Color snappedColor = Color.green;
    public Color lineColor = Color.cyan;
    // 同物体插件引用
    private BlockProperties blockProperties;
    private EditorManager editorManager;
    
    void OnDrawGizmos()
    {
        if (!debugDrawAnchors || bottomAnchors == null || bottomAnchors.Count == 0)
            return;

        Gizmos.color = lineColor;

        foreach (var anchor in bottomAnchors)
        {
            if (anchor == null) continue;

            Vector3 anchorPos = anchor.position;
            Vector3 snappedPos = new Vector3(
                Mathf.Round(anchor.position.x),
                targetY,
                Mathf.Round(anchor.position.z)
            );

            // 绘制 anchor 点
            Gizmos.color = anchorColor;
            Gizmos.DrawSphere(anchorPos, 0.05f);

            // 绘制吸附目标点
            Gizmos.color = snappedColor;
            Gizmos.DrawCube(snappedPos, Vector3.one * 0.1f);

            // 绘制连线
            Gizmos.color = lineColor;
            Gizmos.DrawLine(anchorPos, snappedPos);
        }
    }

    private bool TrySnapToAnchorsGrid()
    {
        if (bottomAnchors == null || bottomAnchors.Count == 0) return false;
        List<Vector3Int> proposedGridCells = new List<Vector3Int>();
        Vector3 moveDelta = Vector3.zero;
        // 预加载要保留的cell坐标
        foreach (var anchor in bottomAnchors)
        {
            Vector3 snapped = new Vector3(
                Mathf.Round(anchor.position.x),
                targetY,
                Mathf.Round(anchor.position.z)
            );

            Vector3 offset = anchor.position - snapped;
            if (moveDelta == Vector3.zero)
                moveDelta = offset;

            Vector3 anchorSnappedWorld = anchor.position - offset;
            Vector3Int gridPos = Vector3Int.RoundToInt(anchorSnappedWorld);
            proposedGridCells.Add(gridPos);
        }
        // 检查是否所有 proposed 格子都未被其他占用（除去自己之前的 reg）
        foreach (var cell in proposedGridCells)
        {
            if (!regPositions.Contains(cell) && GridManager.Instance.IsOccupied(cell))
            {
                Debug.Log("发现放置目标被占用 " + cell);
                return false;
            }
        }

        // 应用移动吸附
        transform.position -= moveDelta;
        // 释放之前占用
        foreach (var cell in regPositions)
        {
            GridManager.Instance.RemoveOccupied(cell);
        }
        // 注册新的占用
        foreach (var cell in proposedGridCells)
        {
            GridManager.Instance.SetOccupied(cell, blockProperties);
        }
        // 更新记录
        regPositions = proposedGridCells;
        // 更新NavMesh
        NavMeshSurface navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
        
        return true;
    }
    

    void Start()
    {
        // 自身应该存在属性
        // 属性设置应该在snapToAnchor 之前
        if (!gameObject.TryGetComponent<BlockProperties>(out blockProperties))
            Debug.LogError("DragAndSnapWithAnchors: BlockProperties 无法找到");
        
        cam = Camera.main;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (editorManager == null) editorManager = FindFirstObjectByType<EditorManager>();
        if (autoSnapOnStart)
        {
            if (!TrySnapToAnchorsGrid())
            {
                Debug.LogError("[DragAndSnap] 放置出错，物体放置坐标存在占用，请检查占用，先已经删除物体");
                Destroy(this);
            }
        }
        
    }
    
    
    void Update()
    {
        // 只有编辑模式中可以拖动
        if (editorManager.inEditMode)
        {   
            // 如果鼠标按下 开始拖动
            if (Input.GetMouseButtonDown(0))
            {
                if (IsMouseOverSelf())
                {
                    isDragging = true;
                    startPosition = transform.position;
                }
            }
            // 如果正在拖动
            if (isDragging)
            {
                if (!IsMouseInScreen(Input.mousePosition))
                {
                    CancelDrag();
                    return;
                }

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 hit = ray.GetPoint(enter);
                    transform.position = new Vector3(hit.x, transform.position.y, hit.z);
                }
            }
            // 如果结束拖动
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                Debug.Log("尝试放置Cube " + gameObject.name);
                isDragging = false;
                if (!TrySnapToAnchorsGrid())
                    CancelDrag();
            }
        }
        else // 如果不在编辑模式，但是当前缺在拖动
        {   
            // 取消拖动
            if (isDragging) CancelDrag();
        }
    }

    private void CancelDrag()
    {
        transform.position = startPosition;
        isDragging = false;
    }

    private bool IsMouseOverSelf()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform;
    }

    private bool IsMouseInScreen(Vector3 pos)
    {
        return pos.x >= 0 && pos.x <= Screen.width && pos.y >= 0 && pos.y <= Screen.height;
    }
}
