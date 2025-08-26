using UnityEngine;

public interface IDraggable : ISelectable
{
    void OnDragStart();
    void OnDragEnd();
    void OnDrag();
    bool IsBeingDragged { get; }
}