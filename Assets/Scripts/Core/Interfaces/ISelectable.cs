using UnityEngine;

public interface ISelectable{
    bool CanBeSelected {get; set;}
    Outline Outline {get; set;}
    void OnSelect();
    void OnDeselect();
    void HighlightSelf();
    void UnhighlightSelf();

}