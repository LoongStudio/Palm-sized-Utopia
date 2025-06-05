using UnityEngine;

/// 输入管理器
public class InputManager : MonoBehaviour
{
    [SerializeField] private PlacementSettings settings;
    [SerializeField] private Camera playerCamera;
    
    private IDragHandler_Utopia dragHandler;
    private bool isEditMode = false;
    
    // 静态引用供其他系统使用
    public static bool IsEditMode { get; private set; }
    public static bool IsDragging { get; private set; }
    
    private void Awake()
    {
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
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var placeable = hit.collider.GetComponent<IPlaceable>();
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
    
    private void ToggleEditMode()
    {
        isEditMode = !isEditMode;
        Debug.Log($"[InputManager] Edit Mode: {(isEditMode ? "ON" : "OFF")}");
        
        // 通知其他系统
        var placementManager = FindObjectOfType<PlacementManager>();
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
