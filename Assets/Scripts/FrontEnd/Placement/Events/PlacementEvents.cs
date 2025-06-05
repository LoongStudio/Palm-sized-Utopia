using System;
using UnityEngine;

/// 放置系统事件
public static class PlacementEvents
{
    // 放置事件
    public static event Action<IPlaceable> OnObjectPlaced;
    public static event Action<IPlaceable> OnObjectRemoved;
    public static event Action<IPlaceable> OnObjectMoved;
    
    // 网格事件
    public static event Action<Vector3Int[]> OnGridOccupationChanged;
    public static event Action<Vector3Int[]> OnGridReleased;
    
    // 拖拽事件
    public static event Action<IPlaceable> OnDragStarted;
    public static event Action<IPlaceable> OnDragEnded;
    public static event Action<IPlaceable> OnDragCancelled;
    
    // 预览事件
    public static event Action<IPlaceable, Vector3Int[]> OnPreviewUpdated;
    public static event Action OnPreviewCleared;
    
    // 触发方法
    public static void TriggerObjectPlaced(IPlaceable placeable)
    {
        OnObjectPlaced?.Invoke(placeable);
    }
    
    public static void TriggerObjectRemoved(IPlaceable placeable)
    {
        OnObjectRemoved?.Invoke(placeable);
    }
    
    public static void TriggerObjectMoved(IPlaceable placeable)
    {
        OnObjectMoved?.Invoke(placeable);
    }
    
    public static void TriggerGridOccupationChanged(Vector3Int[] positions)
    {
        OnGridOccupationChanged?.Invoke(positions);
    }
    
    public static void TriggerGridReleased(Vector3Int[] positions)
    {
        OnGridReleased?.Invoke(positions);
    }
    
    public static void TriggerDragStarted(IPlaceable placeable)
    {
        OnDragStarted?.Invoke(placeable);
    }
    
    public static void TriggerDragEnded(IPlaceable placeable)
    {
        OnDragEnded?.Invoke(placeable);
    }
    
    public static void TriggerDragCancelled(IPlaceable placeable)
    {
        OnDragCancelled?.Invoke(placeable);
    }
    
    public static void TriggerPreviewUpdated(IPlaceable placeable, Vector3Int[] positions)
    {
        OnPreviewUpdated?.Invoke(placeable, positions);
    }
    
    public static void TriggerPreviewCleared()
    {
        OnPreviewCleared?.Invoke();
    }
}
