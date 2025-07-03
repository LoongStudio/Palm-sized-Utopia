using UnityEngine;
using System.Collections;

/// 放置系统主管理器
public class PlacementManager : SingletonManager<PlacementManager>
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("系统设置")]
    [SerializeField] private PlacementSettings settings;
    
    [Header("系统组件")]
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private DragHandler dragHandler;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PreviewManager previewManager;
    [SerializeField] private NavMeshIntegration navMeshIntegration;
    
    // 属性
    public PlacementSettings Settings => settings;
    public IGridSystem GridSystem => gridSystem;
    public IDragHandler_Utopia DragHandler => dragHandler;
    
    // 系统状态
    public bool IsEditMode { get; private set; }
    public bool IsInitialized { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        // 验证配置
        if (settings == null)
        {
            Debug.LogError("[PlacementManager] PlacementSettings not assigned!");
            return;
        }
        
        // 自动获取组件
        AutoGetComponents();
        
        // 初始化系统
        InitializeSystems();
    }
    
    private void Start()
    {
        SubscribeEvents();
        StartCoroutine(DelayedInitialization());
    }
    
    private void SubscribeEvents()
    {
        GameEvents.OnBuildingBought += OnBuildingBought;
    }

    private void OnBuildingBought(BuildingEventArgs args)
    {
        Debug.Log($"[PlacementManager] Building bought: {args.building.name}");
        SetEditMode(true);
    }
    
    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForEndOfFrame();
        
        // 确保所有系统都已初始化
        ValidateSystems();
        
        IsInitialized = true;
        if(showDebugInfo)
            Debug.Log("[PlacementManager] Placement system initialized successfully");
    }
    
    private void AutoGetComponents()
    {
        if (gridSystem == null)
            gridSystem = GetComponent<GridSystem>();
            
        if (dragHandler == null)
            dragHandler = GetComponent<DragHandler>();
            
        if (inputManager == null)
            inputManager = GetComponent<InputManager>();
            
        if (previewManager == null)
            previewManager = GetComponent<PreviewManager>();
            
        if (navMeshIntegration == null)
            navMeshIntegration = GetComponent<NavMeshIntegration>();
    }
    
    private void InitializeSystems()
    {
        // 初始化各个子系统
        if (gridSystem != null)
        {
            if(showDebugInfo)
                Debug.Log("[PlacementManager] GridSystem initialized");
        }
        
        if (dragHandler != null)
        {
            if(showDebugInfo)
                Debug.Log("[PlacementManager] DragHandler initialized");
        }
        
        if (inputManager != null)
        {
            if(showDebugInfo)
                Debug.Log("[PlacementManager] InputManager initialized");
        }
    }
    
    private void ValidateSystems()
    {
        bool hasErrors = false;
        
        if (gridSystem == null)
        {
            Debug.LogError("[PlacementManager] GridSystem is missing!");
            hasErrors = true;
        }
        
        if (dragHandler == null)
        {
            Debug.LogError("[PlacementManager] DragHandler is missing!");
            hasErrors = true;
        }
        
        if (inputManager == null)
        {
            Debug.LogError("[PlacementManager] InputManager is missing!");
            hasErrors = true;
        }
        
        if (hasErrors)
        {
            Debug.LogError("[PlacementManager] System validation failed! Some components are missing.");
        }
    }
    
    /// <summary>
    /// 设置编辑模式
    /// </summary>
    public void SetEditMode(bool enabled)
    {
        IsEditMode = enabled;
        
        // 如果退出编辑模式时正在拖拽，取消拖拽
        if (!enabled && dragHandler.IsDragging)
        {
            dragHandler.CancelDrag();
        }
        
        if(showDebugInfo)
            Debug.Log($"[PlacementManager] Edit Mode: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    /// <summary>
    /// 清空所有放置的物体
    /// </summary>
    public void ClearAllPlacements()
    {
        if (gridSystem != null)
        {
            // 获取所有已放置的物体
            var allPositions = gridSystem.GetAllOccupiedPositions();
            
            foreach (var pos in allPositions)
            {
                var placeable = gridSystem.GetPlaceableAt(pos);
                if (placeable != null)
                {
                    placeable.RemoveFromGrid();
                    
                    // 销毁GameObject（如果需要）
                    var go = (placeable as MonoBehaviour)?.gameObject;
                    if (go != null)
                    {
                        DestroyImmediate(go);
                    }
                }
            }
            
            // 清空网格
            gridSystem.ClearAll();
            
            if(showDebugInfo)
                Debug.Log("[PlacementManager] All placements cleared");
        }
    }
    
    /// <summary>
    /// 获取指定位置的可放置物体
    /// </summary>
    public IPlaceable GetPlaceableAt(Vector3 worldPosition)
    {
        if (gridSystem == null) return null;
        
        var gridPos = gridSystem.WorldToGrid(worldPosition);
        return gridSystem.GetPlaceableAt(gridPos);
    }
    
    /// <summary>
    /// 检查指定区域是否可以放置
    /// </summary>
    public bool CanPlaceAt(Vector3 worldPosition, Vector3Int size)
    {
        if (gridSystem == null) return false;
        
        var baseGridPos = gridSystem.WorldToGrid(worldPosition);
        
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                var checkPos = baseGridPos + new Vector3Int(x, 0, z);
                if (gridSystem.IsOccupied(checkPos))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 强制更新所有系统
    /// </summary>
    public void ForceUpdateAllSystems()
    {
        if (navMeshIntegration != null)
        {
            navMeshIntegration.ForceUpdateNavMesh();
        }
        
        Debug.Log("[PlacementManager] All systems force updated");
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Test Placement System")]
    private void TestPlacementSystem()
    {
        Debug.Log($"[PlacementManager] Testing placement system...");
        Debug.Log($"Is Initialized: {IsInitialized}");
        Debug.Log($"Is Edit Mode: {IsEditMode}");
        Debug.Log($"Grid Size: {gridSystem?.GridSize}");
        Debug.Log($"Is Dragging: {dragHandler?.IsDragging}");
    }
    #endif
}
