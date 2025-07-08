using UnityEngine;
using System.Collections.Generic;

/// 可放置物体接口
public interface IPlaceable
{
    /// 获取占用的网格位置
    Vector3Int[] GetOccupiedPositions();
    
    /// 检查是否可以放置在指定位置
    bool CanPlaceAt(Vector3Int[] positions);
    
    /// 在指定位置放置
    void PlaceAt(Vector3Int[] positions);
    
    /// 移除放置
    void RemoveFromGrid();
    
    /// 获取预览位置（拖拽时）
    Vector3Int[] GetPreviewPositions(Vector3 worldPosition);

    /// 获取地皮类型
    PlaceableType PlaceableType { get; }

    /// 是否已放置
    bool IsPlaced { get; }

    
    /// 放置事件
    System.Action<IPlaceable> OnPlaced { get; set; }
    
    /// 移除事件
    System.Action<IPlaceable> OnRemoved { get; set; }
}
