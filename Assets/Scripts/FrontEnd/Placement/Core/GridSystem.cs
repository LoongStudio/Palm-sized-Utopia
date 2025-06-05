using UnityEngine;
using System.Collections.Generic;
using System.Linq;


/// 网格系统核心实现
public class GridSystem : MonoBehaviour, IGridSystem
{
    [SerializeField] private PlacementSettings settings;
    
    // 网格占用数据
    private readonly HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
    private readonly Dictionary<Vector3Int, IPlaceable> cellOwners = new Dictionary<Vector3Int, IPlaceable>();
    private readonly Dictionary<IPlaceable, Vector3Int[]> placeablePositions = new Dictionary<IPlaceable, Vector3Int[]>();
    
    // 属性
    public float GridSize => settings.GridSize;
    
    private void Awake()
    {
        if (settings == null)
        {
            Debug.LogError("[GridSystem] PlacementSettings not assigned!");
        }
    }
    
    public bool IsOccupied(Vector3Int position)
    {
        return occupiedCells.Contains(position);
    }
    
    public bool AreOccupied(Vector3Int[] positions)
    {
        return positions.Any(pos => occupiedCells.Contains(pos));
    }
    
    public bool TryReserve(Vector3Int[] positions, IPlaceable placer)
    {
        if (positions == null || positions.Length == 0)
            return false;
            
        // 检查是否有冲突（排除自己已占用的位置）
        var currentPositions = placeablePositions.ContainsKey(placer) ? placeablePositions[placer] : new Vector3Int[0];
        var conflictPositions = positions.Where(pos => occupiedCells.Contains(pos) && !currentPositions.Contains(pos));
        
        if (conflictPositions.Any())
            return false;
        
        // 释放之前的位置
        if (placeablePositions.ContainsKey(placer))
        {
            Release(placeablePositions[placer]);
        }
        
        // 占用新位置
        foreach (var pos in positions)
        {
            occupiedCells.Add(pos);
            cellOwners[pos] = placer;
        }
        
        placeablePositions[placer] = positions.ToArray();
        
        // 触发事件
        PlacementEvents.TriggerGridOccupationChanged(positions);
        
        return true;
    }
    
    public void Release(Vector3Int[] positions)
    {
        if (positions == null) return;
        
        foreach (var pos in positions)
        {
            occupiedCells.Remove(pos);
            cellOwners.Remove(pos);
        }
        
        // 从placeablePositions中移除
        var toRemove = placeablePositions.Where(kvp => kvp.Value.SequenceEqual(positions)).ToList();
        foreach (var kvp in toRemove)
        {
            placeablePositions.Remove(kvp.Key);
        }
        
        // 触发事件
        PlacementEvents.TriggerGridReleased(positions);
    }
    
    public IPlaceable GetPlaceableAt(Vector3Int position)
    {
        return cellOwners.TryGetValue(position, out var placeable) ? placeable : null;
    }
    
    public Vector3Int WorldToGrid(Vector3 worldPosition)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPosition.x / GridSize),
            Mathf.RoundToInt(worldPosition.y / GridSize),
            Mathf.RoundToInt(worldPosition.z / GridSize)
        );
    }
    
    public Vector3 GridToWorld(Vector3Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * GridSize,
            gridPosition.y * GridSize,
            gridPosition.z * GridSize
        );
    }
    
    /// <summary>
    /// 获取某个物体占用的所有位置
    /// </summary>
    public Vector3Int[] GetOccupiedPositions(IPlaceable placeable)
    {
        return placeablePositions.TryGetValue(placeable, out var positions) ? positions : new Vector3Int[0];
    }
    
    /// <summary>
    /// 清空所有占用
    /// </summary>
    public void ClearAll()
    {
        occupiedCells.Clear();
        cellOwners.Clear();
        placeablePositions.Clear();
    }
    
    /// <summary>
    /// 获取所有占用的位置
    /// </summary>
    public Vector3Int[] GetAllOccupiedPositions()
    {
        return occupiedCells.ToArray();
    }
    
    private void OnDrawGizmos()
    {
        if (settings == null || !settings.ShowGridGizmos) return;
        
        // 绘制占用的格子
        Gizmos.color = Color.red;
        foreach (var pos in occupiedCells)
        {
            var worldPos = GridToWorld(pos);
            Gizmos.DrawWireCube(worldPos, Vector3.one * GridSize);
        }
    }

}
