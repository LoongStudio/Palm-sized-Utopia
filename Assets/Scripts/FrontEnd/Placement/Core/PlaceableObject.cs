using UnityEngine;
using System.Linq;

/// 可放置物体组件
public class PlaceableObject : MonoBehaviour, IPlaceable
{
    [Header("放置设置")]
    [SerializeField] private Transform[] anchorPoints;
    [SerializeField] private bool autoCalculateAnchors = true;
    [SerializeField] private Vector3Int customSize = Vector3Int.one;
    
    [Header("锚点调试")]
    [SerializeField] private bool showAnchorGizmos = true;
    [SerializeField] private float anchorGizmoSize = 0.2f;

    [Header("预览设置")]
    [SerializeField] private GameObject previewPrefab;
    [SerializeField] private Renderer[] renderers;
    private bool isHiddenForDrag = false;
    
    private IGridSystem gridSystem;
    private Vector3Int[] currentPositions;
    private GameObject previewInstance;
    private Material[] originalMaterials;
    private PlacementSettings settings;
    
    // 属性
    public bool IsPlaced { get; private set; }
    public System.Action<IPlaceable> OnPlaced { get; set; }
    public System.Action<IPlaceable> OnRemoved { get; set; }
    
    private void Awake()
    {
        // 自动获取组件
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
        
        // 保存原始材质
        SaveOriginalMaterials();
        
        // 自动计算锚点
        if (autoCalculateAnchors && (anchorPoints == null || anchorPoints.Length == 0))
        {
            CalculateAnchorPoints();
        }
    }
    
    private void Start()
    {
        // 获取系统引用
        gridSystem = FindAnyObjectByType<GridSystem>();
        settings = FindAnyObjectByType<PlacementManager>()?.Settings;
        
        if (gridSystem == null)
        {
            Debug.LogError("[PlaceableObject] GridSystem not found!");
        }

        // 如果物体已经在场景中，自动注册到网格系统
        if (!IsPlaced && gridSystem != null)
        {
            AutoRegisterToGrid();
        }
    }

    /// <summary>
    /// 自动注册到网格系统（用于预先放置在场景中的物体）
    /// </summary>
    private void AutoRegisterToGrid()
    {
        if (anchorPoints == null || anchorPoints.Length == 0)
        {
            if (autoCalculateAnchors)
            {
                CalculateAnchorPoints();
            }
            else
            {
                Debug.LogWarning($"[PlaceableObject] {name} 没有锚点，无法自动注册");
                return;
            }
        }
        
        // 1. 基于当前位置计算最近的网格位置
        var currentPositions = GetPreviewPositions(transform.position);
        
        // 2. 先对齐到网格位置
        if (currentPositions.Length > 0)
        {
            SnapToGrid(currentPositions);
            
            // 3. 重新计算对齐后的网格位置
            currentPositions = GetPreviewPositions(transform.position);

            // 4. 尝试注册到网格系统
            if (gridSystem.TryReserve(currentPositions, this))
            {
                this.currentPositions = currentPositions;
                IsPlaced = true;

                Debug.Log($"[PlaceableObject] {name} 自动注册并对齐到网格，占用 {currentPositions.Length} 个位置");

                // 同步建筑位置信息
                SyncPositionsToBuilding();

                // 触发放置事件
                TriggerEvents();
            }
            else
            {
                Debug.LogWarning($"[PlaceableObject] {name} 自动注册失败，位置可能有冲突");
            }
        }
    }
    
    public Vector3Int[] GetOccupiedPositions()
    {
        return currentPositions ?? new Vector3Int[0];
    }
    
    /// <summary>
    /// 修正的预览位置计算
    /// </summary>
    public Vector3Int[] GetPreviewPositions(Vector3 worldPosition)
    {
        if (gridSystem == null || anchorPoints == null) 
            return new Vector3Int[0];
        
        var positions = new Vector3Int[anchorPoints.Length];
        
        for (int i = 0; i < anchorPoints.Length; i++)
        {
            // 计算每个锚点在新位置的世界坐标
            var anchorLocalPos = anchorPoints[i].localPosition;
            var anchorWorldPos = worldPosition + transform.TransformVector(anchorLocalPos);
            
            // 转换为网格坐标
            positions[i] = gridSystem.WorldToGrid(anchorWorldPos);
        }
        
        return positions;
    }
    
    public bool CanPlaceAt(Vector3Int[] positions)
    {
        if (gridSystem == null || positions == null || positions.Length == 0)
            return false;
        
        // 检查是否有位置冲突（排除自己当前占用的位置）
        var currentOccupied = GetOccupiedPositions();
        foreach (var pos in positions)
        {
            // 如果位置被占用，且不是自己当前占用的位置，则冲突
            if (gridSystem.IsOccupied(pos) && !System.Array.Exists(currentOccupied, p => p == pos))
            {
                return false;
            }
        }
        
        return true;
    }
    
    public void PlaceAt(Vector3Int[] positions)
    {
        if (gridSystem == null || positions == null) return;
        
        // 尝试预订位置
        if (gridSystem.TryReserve(positions, this))
        {
            currentPositions = positions.ToArray();
            IsPlaced = true;

            // 对齐到网格
            SnapToGrid(positions);

            // 同步建筑位置信息
            SyncPositionsToBuilding();

            // 恢复原始材质
            RestoreOriginalMaterials();

            // 触发事件
            TriggerEvents();

            Debug.Log($"[PlaceableObject] {name} placed at {positions.Length} positions");
        }
        else
        {
            Debug.LogWarning($"[PlaceableObject] Failed to place {name} - positions already occupied");
        }
    }

    private void TriggerEvents()
    {
        OnPlaced?.Invoke(this);
        PlacementEvents.TriggerObjectPlaced(this);
        GameEvents.TriggerBuildingPlaced(new BuildingEventArgs()
        {
            placeable = this,
            building = GetComponentInParent<Building>(),
            eventType = BuildingEventArgs.BuildingEventType.PlaceSuccess,
            timestamp = System.DateTime.Now
        });
    }

    public void RemoveFromGrid()
    {
        if (gridSystem == null || !IsPlaced) return;
        
        gridSystem.Release(currentPositions);
        currentPositions = null;
        IsPlaced = false;
        
        // 同步建筑位置信息（清空）
        SyncPositionsToBuilding();
        
        // 触发事件
        OnRemoved?.Invoke(this);
        PlacementEvents.TriggerObjectRemoved(this);
        
        Debug.Log($"[PlaceableObject] {name} removed from grid");
    }
    
    /// <summary>
    /// 修正的网格对齐方法
    /// </summary>
    private void SnapToGrid(Vector3Int[] gridPositions)
    {
        if (gridPositions.Length == 0 || anchorPoints.Length == 0) return;
        
        // 使用第一个锚点作为基准点进行对齐
        var baseGridPos = gridPositions[0];
        var baseWorldPos = gridSystem.GridToWorld(baseGridPos);
        var anchorLocalOffset = anchorPoints[0].localPosition;
        
        // 计算物体的最终世界位置
        // 需要减去锚点的本地偏移，转换为世界空间偏移
        var worldOffset = transform.TransformVector(anchorLocalOffset);
        var finalPosition = baseWorldPos - worldOffset;
        
        transform.position = finalPosition;
        
        Debug.Log($"[PlaceableObject] 对齐到网格: 基准网格位置 {baseGridPos}, 最终世界位置 {finalPosition}");
    }
    
    /// <summary>
    /// 修正的锚点计算方法 - 考虑父物体Scale的影响
    /// </summary>
    private void CalculateAnchorPoints()
    {
        // 清理旧的锚点
        ClearOldAnchors();
        
        // 获取网格大小和父物体缩放
        var gridSize = GetGridSize();
        var parentScale = transform.localScale;
        
        Debug.Log($"[PlaceableObject] 网格大小: {gridSize}, 父物体缩放: {parentScale}");
        
        // 计算锚点数量
        int anchorCountX = customSize.x;
        int anchorCountZ = customSize.z;
        var anchors = new Transform[anchorCountX * anchorCountZ];
        
        // 计算在本地空间中的实际间距（需要除以父物体的缩放）
        float localSpacingX = gridSize / parentScale.x;
        float localSpacingZ = gridSize / parentScale.z;
        
        Debug.Log($"[PlaceableObject] 本地空间间距: X={localSpacingX}, Z={localSpacingZ}");
        
        // 计算锚点的起始偏移（让锚点网格以物体中心为基准）
        Vector3 startOffset = CalculateStartOffset(anchorCountX, anchorCountZ, localSpacingX, localSpacingZ);
        
        Debug.Log($"[PlaceableObject] 起始偏移: {startOffset}");
        
        int index = 0;
        for (int x = 0; x < anchorCountX; x++)
        {
            for (int z = 0; z < anchorCountZ; z++)
            {
                // 计算锚点的本地位置（使用修正后的间距）
                Vector3 anchorLocalPos = startOffset + new Vector3(
                    x * localSpacingX,  // X方向使用本地空间间距
                    0,
                    z * localSpacingZ   // Z方向使用本地空间间距
                );
                
                // 创建锚点子对象
                var anchorGO = new GameObject($"Anchor_{x}_{z}");
                anchorGO.transform.SetParent(transform);
                anchorGO.transform.localPosition = anchorLocalPos;
                
                anchors[index++] = anchorGO.transform;
                
                // 验证世界空间位置
                var worldPos = anchorGO.transform.position;
                Debug.Log($"[PlaceableObject] 创建锚点 [{x},{z}] 本地位置: {anchorLocalPos}, 世界位置: {worldPos}");
            }
        }
        
        anchorPoints = anchors;
        
        Debug.Log($"[PlaceableObject] 总共创建了 {anchorPoints.Length} 个锚点");
        
        // 验证锚点间的世界距离
        ValidateAnchorSpacing();
    }
    
    /// <summary>
    /// 计算起始偏移，使锚点网格以物体中心为基准
    /// </summary>
    private Vector3 CalculateStartOffset(int countX, int countZ, float localSpacingX, float localSpacingZ)
    {
        // 计算锚点网格在本地空间的总尺寸
        float totalLocalWidth = (countX - 1) * localSpacingX;
        float totalLocalDepth = (countZ - 1) * localSpacingZ;
        
        // 计算起始偏移，使网格居中
        Vector3 offset = new Vector3(
            -totalLocalWidth * 0.5f,   // X轴居中
            0,
            -totalLocalDepth * 0.5f    // Z轴居中
        );
        
        return offset;
    }
    
    /// <summary>
    /// 验证锚点间距是否正确
    /// </summary>
    private void ValidateAnchorSpacing()
    {
        if (anchorPoints == null || anchorPoints.Length < 2) return;
        
        var gridSize = GetGridSize();
        
        // 检查相邻锚点的世界距离
        for (int i = 0; i < anchorPoints.Length - 1; i++)
        {
            if (anchorPoints[i] != null && anchorPoints[i + 1] != null)
            {
                var distance = Vector3.Distance(anchorPoints[i].position, anchorPoints[i + 1].position);
                
                // 对于网格对齐的锚点，距离应该等于网格大小
                if (Mathf.Abs(distance - gridSize) < 0.001f)
                {
                    Debug.Log($"[PlaceableObject] ✅ 锚点 {i} 到 {i + 1} 距离正确: {distance:F3}");
                }
                else if (distance > 0.001f) // 忽略同位置的锚点
                {
                    Debug.Log($"[PlaceableObject] ⚠️ 锚点 {i} 到 {i + 1} 距离: {distance:F3}, 期望: {gridSize}");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取物体的实际边界
    /// </summary>
    private Bounds GetObjectBounds()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[PlaceableObject] {name} 没有找到 Renderer 组件，使用默认边界");
            return new Bounds(transform.position, Vector3.one);
        }
        
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        
        // 转换为本地空间
        bounds.center = transform.InverseTransformPoint(bounds.center);
        bounds.size = transform.InverseTransformVector(bounds.size);
        
        return bounds;
    }
    
    /// <summary>
    /// 获取网格大小
    /// </summary>
    private float GetGridSize()
    {
        var gridSystem = FindAnyObjectByType<GridSystem>();
        return gridSystem != null ? gridSystem.GridSize : 1.0f;
    }
    
    /// <summary>
    /// 清理旧的锚点
    /// </summary>
    private void ClearOldAnchors()
    {
        if (anchorPoints != null)
        {
            foreach (var anchor in anchorPoints)
            {
                if (anchor != null)
                {
                    if (Application.isPlaying)
                        Destroy(anchor.gameObject);
                    else
                        DestroyImmediate(anchor.gameObject);
                }
            }
        }
        anchorPoints = null;
    }
    
    /// <summary>
    /// 在编辑器中重新计算锚点
    /// </summary>
    [ContextMenu("重新计算锚点")]
    public void RecalculateAnchors()
    {
        if (autoCalculateAnchors)
        {
            CalculateAnchorPoints();
            Debug.Log($"[PlaceableObject] {name} 的锚点已重新计算");
        }
    }
    
    /// <summary>
    /// 验证锚点配置
    /// </summary>
    [ContextMenu("验证锚点配置")]
    public void ValidateAnchorConfiguration()
    {
        Debug.Log("=== 锚点配置验证 ===");
        Debug.Log($"物体名称: {name}");
        Debug.Log($"物体缩放: {transform.localScale}");
        Debug.Log($"自定义大小: {customSize}");
        Debug.Log($"锚点数量: {anchorPoints?.Length ?? 0}");
        Debug.Log($"期望锚点数量: {customSize.x * customSize.z}");
        
        if (anchorPoints != null)
        {
            for (int i = 0; i < anchorPoints.Length; i++)
            {
                if (anchorPoints[i] != null)
                {
                    var localPos = anchorPoints[i].localPosition;
                    var worldPos = anchorPoints[i].position;
                    Debug.Log($"锚点 {i}: 本地位置 {localPos}, 世界位置 {worldPos}");
                }
            }
        }
    }

    
    
    private void SaveOriginalMaterials()
    {
        if (renderers == null) return;
        
        originalMaterials = renderers.SelectMany(r => r.materials).ToArray();
    }
    
    private void RestoreOriginalMaterials()
    {
        if (renderers == null || originalMaterials == null) return;
        
        int materialIndex = 0;
        foreach (var renderer in renderers)
        {
            var materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                if (materialIndex < originalMaterials.Length)
                {
                    materials[i] = originalMaterials[materialIndex++];
                }
            }
            renderer.materials = materials;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showAnchorGizmos || anchorPoints == null) return;
        
        // 绘制锚点
        Gizmos.color = IsPlaced ? Color.green : Color.yellow;
        foreach (var anchor in anchorPoints)
        {
            if (anchor != null)
            {
                Gizmos.DrawWireCube(anchor.position, Vector3.one * anchorGizmoSize);
                
                // 绘制锚点到物体中心的连线
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, anchor.position);
                Gizmos.color = IsPlaced ? Color.green : Color.yellow;
            }
        }
        
        // 绘制物体中心
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, anchorGizmoSize * 0.5f);
    }
    
    /// <summary>
    /// 在选中时绘制网格预览
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (gridSystem == null || anchorPoints == null) return;
        
        // 绘制网格预览
        Gizmos.color = Color.cyan;
        foreach (var anchor in anchorPoints)
        {
            if (anchor != null)
            {
                var gridPos = gridSystem.WorldToGrid(anchor.position);
                var worldPos = gridSystem.GridToWorld(gridPos);
                Gizmos.DrawWireCube(worldPos, Vector3.one * gridSystem.GridSize);
            }
        }
    }

    // 添加方法
    public void SetVisibility(bool visible)
    {
        if (renderers == null)
            renderers = GetComponentsInChildren<Renderer>();
            
        foreach (var renderer in renderers)
        {
            renderer.enabled = visible;
        }
        isHiddenForDrag = !visible;
    }
    
    /// <summary>
    /// 同步位置信息到Building组件
    /// </summary>
    private void SyncPositionsToBuilding()
    {
        var building = GetComponentInChildren<Building>();
        if (building != null)
        {
            building.SyncPositionsFromPlaceable();
        }
    }
}
