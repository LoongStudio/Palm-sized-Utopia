using UnityEngine;

/// 拖拽处理接口
public interface IDragHandler_Utopia
{
    /// 开始拖拽
    void StartDrag(IPlaceable target, Building building = null, bool isNewlyCreated = false, bool isNewlyBought = false);
    
    /// 更新拖拽位置
    void UpdateDrag(Vector3 worldPosition);
    
    /// 结束拖拽
    bool EndDrag();
    
    /// 取消拖拽
    void CancelDrag();
    
    /// 是否正在拖拽
    bool IsDragging { get; }
    
    /// 当前拖拽目标
    IPlaceable CurrentTarget { get; }
}