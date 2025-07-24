using UnityEngine;

public interface ISelectable{
    Outline Outline {get; set;}
    void OnSelect();
    void OnDeselect();
    void HighlightSelf();
    void UnhighlightSelf();

}