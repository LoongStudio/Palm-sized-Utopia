using UnityEngine;
using System.Collections.Generic;

/// 网格系统接口
public interface IGridSystem
{
    /// 检查位置是否被占用
    bool IsOccupied(Vector3Int position);
    
    /// 批量检查位置是否被占用
    bool AreOccupied(Vector3Int[] positions);
    
    /// 尝试预订位置
    bool TryReserve(Vector3Int[] positions, IPlaceable placer);
    
    /// 释放位置
    void Release(Vector3Int[] positions);
    
    /// 获取位置上的物体
    IPlaceable GetPlaceableAt(Vector3Int position);
    
    /// 世界坐标转网格坐标
    Vector3Int WorldToGrid(Vector3 worldPosition);
    
    /// 网格坐标转世界坐标
    Vector3 GridToWorld(Vector3Int gridPosition);
    
    /// 网格大小
    float GridSize { get; }

}