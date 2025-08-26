using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

public class SelectManager : SingletonManager<SelectManager>{
    [SerializeField, LabelText("显示调试信息")]private bool showDebugInfo = false;
    [SerializeField, LabelText("可选择层")]private LayerMask selectableLayers;
    [SerializeField, LabelText("当前选中"), ReadOnly]private GameObject selected = null;
    private IDraggable draggableObject = null;  // 当前可拖动的对象
    
    // 用于区分点击和拖动的字段
    private Vector3 mouseDownPosition;
    private float mouseDownTime;
    private bool isMouseDown = false;
    private bool isDragging = false; // 新增：是否正在拖动
    private const float dragThreshold = 5f; // 拖动阈值（像素）
    private const float clickTimeout = 4f; // 点击超时时间（秒）
    
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
        // 鼠标左键按下
        if (Input.GetMouseButtonDown(0))
        {
            // 检查是否点击在 UI 上（忽略点击）
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;  // 正在点击 UI，忽略
            }
            
            // 发射射线检测场景物体
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, selectableLayers))  // 多层检测
            {
                // 选中物体并高亮显示
                Select(hit.collider.gameObject);
                
                // 记录鼠标按下时的位置和时间
                mouseDownPosition = Input.mousePosition;
                mouseDownTime = Time.time;
                isMouseDown = true;
                isDragging = false; // 重置拖动状态
            }
        }
        
        // 鼠标左键保持按下状态 - 检测拖动
        if (Input.GetMouseButton(0) && isMouseDown)
        {
            // 检查是否点击在 UI 上（忽略点击）
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;  // 正在点击 UI，忽略
            }
            
            // 计算鼠标移动距离
            float distance = Vector3.Distance(Input.mousePosition, mouseDownPosition);
            
            // 如果移动距离超过阈值，开始拖动
            if (distance > dragThreshold && !isDragging)
            {
                // 检查当前选中的对象是否实现了IDraggable接口
                if (selected != null)
                {
                    draggableObject = selected.GetComponentInChildren<IDraggable>();
                    if (draggableObject != null)
                    {
                        draggableObject.OnDragStart();
                        isDragging = true;
                    }
                }
            }
            
            // 处理拖动逻辑
            if (isDragging && draggableObject != null)
            {
                draggableObject.OnDrag();
            }
        }
        
        // 鼠标左键释放
        if (Input.GetMouseButtonUp(0) && isMouseDown)
        {
            // 检查是否点击在 UI 上（忽略点击）
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                isMouseDown = false;
                return;  // 正在点击 UI，忽略
            }
            
            // 如果正在拖动，则结束拖动
            if (isDragging)
            {
                if (draggableObject != null)
                {
                    draggableObject.OnDragEnd();
                    draggableObject = null;
                }
            }
            else
            {
                // 计算按下时间
                float timeElapsed = Time.time - mouseDownTime;
                
                // 如果按下时间小于超时时间，则认为是点击，打开UI
                if (timeElapsed <= clickTimeout)
                {
                    // 触发UI打开事件
                    if (selected != null)
                    {
                        ISelectable selectable = selected.GetComponentInChildren<ISelectable>();
                        if (selectable != null)
                        {
                            // 这里可以触发UI打开事件
                            GameEvents.TriggerNPCSelected(selectable as NPC);
                        }
                    }
                }
            }
            
            // 重置鼠标按下状态
            isMouseDown = false;
            isDragging = false;
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