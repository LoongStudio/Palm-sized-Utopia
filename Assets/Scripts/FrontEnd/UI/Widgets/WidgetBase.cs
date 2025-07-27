using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public abstract class WidgetBase : MonoBehaviour{
    public abstract void UpdateSelf();
    protected virtual void AutoAdjustTextWidth(TextMeshProUGUI text){
        LayoutElement layout = text.GetComponent<LayoutElement>();
        if(layout == null){
            layout = text.gameObject.AddComponent<LayoutElement>();
        }
        layout.preferredWidth = text.preferredWidth;
    }
}