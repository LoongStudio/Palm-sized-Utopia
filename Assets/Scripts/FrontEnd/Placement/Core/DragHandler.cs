using UnityEngine;
using System.Collections;

/// 拖拽处理器
public class DragHandler : MonoBehaviour, IDragHandler_Utopia
{
    [SerializeField] private PlacementSettings settings;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask groundLayer;
    
    private IPlaceable currentTarget;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 dragOffset;
    private Vector3 velocity;
    private Plane groundPlane;
    
    // 属性
    public bool IsDragging { get; private set; }
    public IPlaceable CurrentTarget => currentTarget;
    
    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }
    
    public void StartDrag(IPlaceable target)
    {
        if (target == null || IsDragging) return;
        
        currentTarget = target;
        IsDragging = true;
        
        // 记录原始状态
        var targetTransform = GetTransform(target);
        if (targetTransform != null)
        {
            originalPosition = targetTransform.position;
            originalRotation = targetTransform.rotation;
            
            // 计算拖拽偏移
            var mouseWorldPos = GetMouseWorldPosition();
            dragOffset = originalPosition - mouseWorldPos;
        }

        // 隐藏原物体
        if (target is PlaceableObject placeableObj)
        {
            placeableObj.SetVisibility(false);
        }
        
        // 触发事件
        PlacementEvents.TriggerDragStarted(target);
        
        Debug.Log($"[DragHandler] Started dragging {target}");
    }
    
    public void UpdateDrag(Vector3 worldPosition)
    {
        if (!IsDragging || currentTarget == null) return;
        
        var targetTransform = GetTransform(currentTarget);
        if (targetTransform == null) return;
        
        // 计算目标位置
        var targetPosition = worldPosition + dragOffset;
        
        // 应用平滑移动
        if (settings.DragSmoothTime > 0)
        {
            targetPosition = Vector3.SmoothDamp(
                targetTransform.position,
                targetPosition,
                ref velocity,
                settings.DragSmoothTime
            );
        }
        
        // 应用地面吸附
        targetPosition = SnapToGround(targetPosition);
        
        // 更新位置
        targetTransform.position = targetPosition;
        
        // 更新预览
        var previewPositions = currentTarget.GetPreviewPositions(targetPosition);
        PlacementEvents.TriggerPreviewUpdated(currentTarget, previewPositions);
    }
    
    public bool EndDrag()
    {
        if (!IsDragging || currentTarget == null) return false;
        
        var targetTransform = GetTransform(currentTarget);
        if (targetTransform == null)
        {
            CancelDrag();
            return false;
        }
        
        // 尝试放置
        var previewPositions = currentTarget.GetPreviewPositions(targetTransform.position);
        bool canPlace = currentTarget.CanPlaceAt(previewPositions);
        
        if (canPlace)
        {
            // 放置成功
            currentTarget.PlaceAt(previewPositions);
            PlacementEvents.TriggerDragEnded(currentTarget);
            
            Debug.Log($"[DragHandler] Successfully placed {currentTarget}");
        }
        else
        {
            // 放置失败，恢复原位置
            targetTransform.position = originalPosition;
            targetTransform.rotation = originalRotation;
            
            Debug.Log($"[DragHandler] Failed to place {currentTarget}, restored to original position");
        }
        
        // 清理状态
        CleanupDrag();
        
        return canPlace;
    }
    
    public void CancelDrag()
    {
        if (!IsDragging) return;
        
        // 恢复原位置
        var targetTransform = GetTransform(currentTarget);
        if (targetTransform != null)
        {
            targetTransform.position = originalPosition;
            targetTransform.rotation = originalRotation;
        }
        
        PlacementEvents.TriggerDragCancelled(currentTarget);
        
        Debug.Log($"[DragHandler] Cancelled dragging {currentTarget}");
        
        CleanupDrag();
    }
    
    private void CleanupDrag()
    {
        // 恢复原物体显示
        if (currentTarget is PlaceableObject placeableObj)
        {
            placeableObj.SetVisibility(true);
        }
            
        PlacementEvents.TriggerPreviewCleared();
        currentTarget = null;
        IsDragging = false;
        velocity = Vector3.zero;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        if (playerCamera == null) return Vector3.zero;
        
        var ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        return Vector3.zero;
    }
    
    private Vector3 SnapToGround(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            return new Vector3(position.x, hit.point.y + settings.GroundSnapHeight, position.z);
        }
        
        return position;
    }
    
    private Transform GetTransform(IPlaceable placeable)
    {
        return (placeable as MonoBehaviour)?.transform;
    }
    
    private void Update()
    {
        if (IsDragging)
        {
            // 更新拖拽位置
            var mouseWorldPos = GetMouseWorldPosition();
            UpdateDrag(mouseWorldPos);
            
            // 检查取消按键
            if (Input.GetKeyDown(settings.CancelKey))
            {
                CancelDrag();
            }
        }
    }
}
