using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

/// 拖拽处理器
public class DragHandler : SingletonManager<DragHandler>, IDragHandler_Utopia
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
    
    private bool isNewlyBoughtBuilding = false;
    private Building currentDraggingBuilding;
    private bool isEndingDrag = false; // 防止重复调用EndDrag
    
    // 属性
    public bool IsDragging { get; private set; }
    public IPlaceable CurrentTarget => currentTarget;
    public bool IsNewlyBoughtBuilding => isNewlyBoughtBuilding;
    
    protected override void Awake()
    {
        base.Awake();
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }
    private void Start()
    {
        SubscribeEvents();
    }
    private void SubscribeEvents()
    {
        GameEvents.OnBuildingBought += OnBuildingBought;
    }
    private void OnBuildingBought(BuildingEventArgs args)
    {
        Debug.Log($"[DragHandler] Building bought: {args.building.name}");
        var placeable = args.building.GetComponentInParent<IPlaceable>();
        if (placeable != null)
        {
            StartDrag(placeable, args.building, true, true);
        }
        else
        {
            Debug.LogError($"[DragHandler] Building没有实现IPlaceable接口: {args.building.name}");
        }
    }
    public void StartDrag(IPlaceable target, Building building = null, bool isNewlyCreated = false, bool isNewlyBought = false)
    {
        if (target == null || IsDragging) return;
        
        currentTarget = target;
        IsDragging = true;
        isNewlyBoughtBuilding = isNewlyCreated;
        currentDraggingBuilding = building;
        // 记录原始状态
        var targetTransform = GetTransform(target);
        if (targetTransform != null)
        {
            if (isNewlyCreated)
            {
                // 对于新创建的建筑，直接设置到鼠标位置
                var mouseWorldPos = GetMouseWorldPosition();
                targetTransform.position = mouseWorldPos;
                originalPosition = mouseWorldPos;
                dragOffset = Vector3.zero; // 新建筑不需要偏移
            }
            else
            {
                // 对于已存在的建筑，使用原有的偏移计算
                originalPosition = targetTransform.position;
                originalRotation = targetTransform.rotation;
                
                var mouseWorldPos = GetMouseWorldPosition();
                dragOffset = originalPosition - mouseWorldPos;
            }
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
        if (!IsDragging || currentTarget == null || isEndingDrag) return false;
        
        // 设置标志防止重复调用
        isEndingDrag = true;
        Debug.Log($"[DragHandler] EndDrag called - preventing duplicate calls");
        
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
            if (isNewlyBoughtBuilding)
            {
                // 新创建建筑放置成功
                currentTarget.PlaceAt(previewPositions);
                PlacementEvents.TriggerDragEnded(currentTarget);
                // 触发新创建建筑放置成功事件
                GameEvents.TriggerBoughtBuildingPlacedAfterDragging(new BuildingEventArgs()
                {
                    building = currentDraggingBuilding,
                    placeable = currentTarget,
                    placeableType = currentTarget.PlaceableType,
                    eventType = BuildingEventArgs.BuildingEventType.PlaceSuccess,
                    timestamp = System.DateTime.Now
                });
            }
            else
            {
                // 已存在的建筑放置成功
                currentTarget.PlaceAt(previewPositions);
                PlacementEvents.TriggerDragEnded(currentTarget);
                // TODO：触发已存在建筑放置成功事件
            }
            
            Debug.Log($"[DragHandler] Successfully placed {currentTarget}");
        }
        else
        {
            if (isNewlyBoughtBuilding)
            {
                // 新创建建筑放置失败时
                GameEvents.TriggerBoughtBuildingPlacedAfterDragging(new BuildingEventArgs(){
                    building = currentDraggingBuilding,
                    placeable = currentTarget,
                    placeableType = currentTarget.PlaceableType,
                    eventType = BuildingEventArgs.BuildingEventType.PlaceFailed,
                    timestamp = System.DateTime.Now
                });

                Debug.LogWarning($"[DragHandler] 新建筑放置失败");
            }
            else
            {
                // 放置失败，恢复原位置
                targetTransform.position = originalPosition;
                targetTransform.rotation = originalRotation;

                Debug.Log($"[DragHandler] Failed to place {currentTarget}, restored to original position:{originalPosition}");
            }
        }
        
        // 清理状态 - 这里会将IsDragging设为false，防止重复调用
        CleanupDrag();
        
        return canPlace;
    }
    
    public void CancelDrag()
    {
        if (!IsDragging) return;
        
        // 设置标志防止重复调用
        isEndingDrag = true;
        
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
        isNewlyBoughtBuilding = false;
        currentDraggingBuilding = null;
        isEndingDrag = false; // 重置标志
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
            
            // 对于新建建筑，检查左键点击来放置
            if (isNewlyBoughtBuilding && Input.GetMouseButtonDown(0))
            {
                Debug.Log($"[DragHandler] Mouse button down detected in Update() - calling EndDrag()");
                EndDrag();
            }
        }
    }
}
