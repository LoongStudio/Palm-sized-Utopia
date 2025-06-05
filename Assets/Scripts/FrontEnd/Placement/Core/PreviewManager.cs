using UnityEngine;
using System.Collections.Generic;

/// 预览管理器
public class PreviewManager : MonoBehaviour
{
    [SerializeField] private PlacementSettings settings;
    [SerializeField] private Material validPreviewMaterial;
    [SerializeField] private Material invalidPreviewMaterial;
    
    private IGridSystem gridSystem;
    private Dictionary<Vector3Int, GameObject> previewObjects = new Dictionary<Vector3Int, GameObject>();
    private GameObject previewParent;
    
    private void Awake()
    {
        // 创建预览对象父节点
        previewParent = new GameObject("Preview Objects");
        previewParent.transform.SetParent(transform);
    }
    
    private void Start()
    {
        gridSystem = FindObjectOfType<GridSystem>();
        
        // 订阅事件
        PlacementEvents.OnPreviewUpdated += ShowPreview;
        PlacementEvents.OnPreviewCleared += ClearPreview;
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        PlacementEvents.OnPreviewUpdated -= ShowPreview;
        PlacementEvents.OnPreviewCleared -= ClearPreview;
    }
    
    private void ShowPreview(IPlaceable placeable, Vector3Int[] positions)
    {
        if (gridSystem == null || positions == null) return;
        
        // 清除旧预览
        ClearPreview();
        
        // 检查是否可以放置
        bool canPlace = placeable.CanPlaceAt(positions);
        Material previewMaterial = canPlace ? validPreviewMaterial : invalidPreviewMaterial;
        
        // 创建新预览
        foreach (var gridPos in positions)
        {
            var worldPos = gridSystem.GridToWorld(gridPos);
            var previewObj = CreatePreviewCube(worldPos, previewMaterial);
            previewObjects[gridPos] = previewObj;
        }
    }
    
    private void ClearPreview()
    {
        foreach (var previewObj in previewObjects.Values)
        {
            if (previewObj != null)
            {
                DestroyImmediate(previewObj);
            }
        }
        previewObjects.Clear();
    }
    
    private GameObject CreatePreviewCube(Vector3 position, Material material)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(previewParent.transform);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * gridSystem.GridSize * 0.9f;
        
        // 移除碰撞器
        var collider = cube.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }
        
        // 设置材质
        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }
        
        return cube;
    }
}
