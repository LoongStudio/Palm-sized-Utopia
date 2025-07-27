using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

public class SelectManager : SingletonManager<SelectManager>{
    [SerializeField, LabelText("显示调试信息")]private bool showDebugInfo = false;
    [SerializeField, LabelText("可选择层")]private LayerMask selectableLayers;
    [SerializeField, LabelText("当前选中"), ReadOnly]private GameObject selected = null;
    public GameObject Selected{
        get{
            return selected;
        }
    }
    private void OnEnable(){
        
    }
    private void OnDisable(){
        
    }
    private void Update()
    {
        // 鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 检查是否点击在 UI 上（忽略点击）
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;  // 正在点击 UI，忽略
            }

            // 2. 发射射线检测场景物体
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, selectableLayers))  // 多层检测
            {
                Select(hit.collider.gameObject);
            }
        }
    }
    public void Select(GameObject selected){
        if(selected == null){
            Debug.LogError("[SelectManager] 选中物体为空");
            return;
        }

        // 取消选中上一个物体
        if(this.selected != null && this.selected != selected){
            ISelectable selectable = this.selected.GetComponentInChildren<ISelectable>();
            if(selectable != null){
                selectable.OnDeselect();
            }
        }
        // 选中当前物体
        this.selected = selected;
        ISelectable newSelectable = selected.GetComponentInChildren<ISelectable>();
        if(newSelectable != null){
            if(newSelectable.CanBeSelected){
                newSelectable.OnSelect();
            }else{
                Debug.LogWarning($"[SelectManager] 选中物体{selected.name}CanBeSelected为false");
            }
        }else{
            Debug.LogWarning($"[SelectManager] 选中物体{selected.name}没有实现ISelectable接口");
        }
    }
}