using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 建筑系统适配器 - 连接新的放置系统和现有建筑系统
/// </summary>
public class BuildingPlaceableAdapter : MonoBehaviour
{
    [Header("建筑引用")]
    [SerializeField] private Building building;
    [SerializeField] private PlaceableObject placeableObject;
    
    private void Awake()
    {
        // 自动获取组件
        if (building == null)
            building = GetComponent<Building>();
            
        if (placeableObject == null)
            placeableObject = GetComponent<PlaceableObject>();
            
        if (building == null || placeableObject == null)
        {
            Debug.LogError("[BuildingPlaceableAdapter] Missing required components!");
            return;
        }
        
        // 设置事件监听
        placeableObject.OnPlaced += OnBuildingPlaced;
        placeableObject.OnRemoved += OnBuildingRemoved;
    }
    
    private void OnDestroy()
    {
        if (placeableObject != null)
        {
            placeableObject.OnPlaced -= OnBuildingPlaced;
            placeableObject.OnRemoved -= OnBuildingRemoved;
        }
    }
    
    private void OnBuildingPlaced(IPlaceable placeable)
    {
        if (building == null) return;
        
        // 转换网格位置到建筑系统的位置格式
        var gridPositions = placeableObject.GetOccupiedPositions();
        var buildingPositions = new List<Vector2Int>();
        
        foreach (var gridPos in gridPositions)
        {
            buildingPositions.Add(new Vector2Int(gridPos.x, gridPos.z));
        }
        
        // 设置建筑位置
        building.positions = buildingPositions;
        
        // 调用建筑的放置逻辑
        if (!building.OnTryBuilt())
        {
            Debug.LogError($"[BuildingPlaceableAdapter] Building {building.name} failed to register with BuildingManager");
        }
        
        Debug.Log($"[BuildingPlaceableAdapter] Building {building.name} placed successfully");
    }
    
    private void OnBuildingRemoved(IPlaceable placeable)
    {
        if (building == null) return;
        
        // 调用建筑的移除逻辑
        building.OnDestroyed();
        
        Debug.Log($"[BuildingPlaceableAdapter] Building {building.name} removed");
    }
    
    /// <summary>
    /// 从建筑数据初始化放置组件
    /// </summary>
    public void InitializeFromBuildingData()
    {
        if (building?.data == null || placeableObject == null) return;
        
        // 根据建筑数据设置放置组件的大小
        var size = building.data.size;
        // 这里可以根据需要调整placeableObject的设置
        
        Debug.Log($"[BuildingPlaceableAdapter] Initialized from building data: {building.data.buildingName}");
    }
}