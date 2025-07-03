using UnityEngine;

/// 输入管理器
public class InputManager : SingletonManager<InputManager>
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    [SerializeField] private PlacementSettings settings;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxDragRaycastDistance  = 100f;
    
    private IDragHandler_Utopia dragHandler;
    private bool isEditMode = false;
    
    // 静态引用供其他系统使用
    public static bool IsEditMode { get; private set; }
    public static bool IsDragging { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        dragHandler = GetComponent<DragHandler>();
        if (dragHandler == null)
        {
            Debug.LogError("[InputManager] DragHandler component not found!");
        }
    }
    
    private void Update()
    {
        HandleEditModeInput();
        
        if (isEditMode)
        {
            HandleDragInput();
            HandleCancelInput();
        }
        
        UpdateStaticProperties();
    }
    
    private void HandleEditModeInput()
    {
        if (Input.GetKeyDown(settings.EditModeKey))
        {
            ToggleEditMode();
        }
    }
    
    private void HandleDragInput()
    {
        if (Input.GetMouseButtonDown(settings.DragMouseButton))
        {
            StartDragFromMouse();
        }
        else if (Input.GetMouseButtonUp(settings.DragMouseButton))
        {
            // 检查是否是新建建筑的拖拽，如果是则不处理鼠标抬起事件
            // 新建建筑的拖拽由DragHandler的Update方法处理
            var dragHandlerComponent = dragHandler as DragHandler;
            if (dragHandlerComponent != null && dragHandlerComponent.IsNewlyBoughtBuilding)
            {
                // 新建建筑的拖拽不在这里处理鼠标抬起事件
                Debug.Log($"[InputManager] Skipping mouse up event for newly bought building");
                return;
            }
            Debug.Log($"[InputManager] Mouse button up detected - calling EndDrag()");
            EndDrag();
        }
    }
    
    private void HandleCancelInput()
    {
        if (Input.GetKeyDown(settings.CancelKey))
        {
            if (dragHandler.IsDragging)
            {
                dragHandler.CancelDrag();
            }
        }
    }
    
    private void StartDragFromMouse()
    {
        if (dragHandler.IsDragging) return;
        
        var ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDragRaycastDistance, settings.DragLayer))
        {
            var placeable = hit.collider.GetComponentInParent<IPlaceable>();
            if (placeable != null)
            {
                dragHandler.StartDrag(placeable);
            }
        }
    }
    
    private void EndDrag()
    {
        if (dragHandler.IsDragging)
        {
            dragHandler.EndDrag();
        }
    }
    
    public void ToggleEditMode()
    {
        isEditMode = !isEditMode;
        if(showDebugInfo)
            Debug.Log($"[InputManager] Edit Mode: {(isEditMode ? "ON" : "OFF")}");
        
        // 通知其他系统
        var placementManager = FindAnyObjectByType<PlacementManager>();
        if (placementManager != null)
        {
            placementManager.SetEditMode(isEditMode);
        }
    }
    
    private void UpdateStaticProperties()
    {
        IsEditMode = isEditMode;
        IsDragging = dragHandler?.IsDragging ?? false;
    }
    
    public void SetEditMode(bool enabled)
    {
        isEditMode = enabled;
    }
}
