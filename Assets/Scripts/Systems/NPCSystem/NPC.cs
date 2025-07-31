using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using UnityEditor;
using Sirenix.OdinInspector;

[System.Serializable]
public class ComponentDisableEntry
{
    public Behaviour component;
    public bool disableDuringDrag = true;
}

public class NPC : MonoBehaviour, ISaveable, IDraggable
{
    #region 字段声明
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("调试设置")]
    public float heightOffset = 0.1f;  // 文本显示高度
    public Color textColor = Color.white;
    public float textSize = 12;
    public bool alwaysShow = true;  // 是否始终显示，否则仅在选中时显示
    private GUIStyle textStyle;
    
    [Header("唯一标识")]
    [SerializeField] private string npcId;

    [Header("基本信息")]
    public NPCData data;
    public HousingBuilding housing; // 添加住房属性

    [Header("社交系统")]
    // public Dictionary<NPC, int> relationships = new Dictionary<NPC, int>(); // 好感度系统
    public Vector3 socialPosition; // 社交位置

    [Header("社交配置")]
    // TODO: 处理这些配置的使用，用SocialSystem代替
    // [SerializeField] private int maxRelationship = 100;
    // [SerializeField] private int minRelationship = 0;
    // [SerializeField] private int defaultRelationship = 50;

    [Header("状态管理")]
    public NPCStateMachine stateMachine;
    public NPCState currentState => stateMachine.CurrentState;
    public NPCState previousState => stateMachine.PreviousState;
    public float stateTimer => stateMachine.GetStateTimer();

    [Header("任务和背包")]
    public Inventory inventory;           // 背包
    public NavMeshAgent navAgent;           // 导航组件
    public int transferSpeed = 10;          // 转移物体的数量
    [Header("移动配置")]
    public NPCMovement movement;
    // 移动相关字段已转移到NPCMovement中

    // 属性访问器，用于向后兼容
    public Transform currentTarget => movement?.CurrentTarget;
    public bool isLanded => movement?.isLanded ?? false;
    public bool isInPosition => movement?.isInPosition ?? false;

    [Header("工作系统")]
    [SerializeField] public TaskInfo pendingTask;       // 待处理的工作建筑
    [SerializeField] private TaskInfo assignedTask;               // 分配的建筑
    [ReadOnly] private bool isLocked = false;
    [SerializeField] public TaskInfo lockedTask = TaskInfo.GetNone();
    [SerializeField] private float idleTimeWeight = 0.1f;        // 每秒增加的权重
    [SerializeField] private float currentIdleWeight = 0f;       // 当前累积的权重
    
    public TaskInfo AssignedTask{get{return assignedTask;}}
    public float CurrentIdleWeight => currentIdleWeight;
    public bool IsLocked => isLocked;


    [Header("拖动设置")]
    [SerializeField] private float dragPlaneHeight = 0f; // 拖动平面的Y轴高度
    [SerializeField] private bool useFixedDragPlane = true; // 是否使用固定高度平面
    
    [Header("拖动期间禁用的组件")]
    [SerializeField] private List<ComponentDisableEntry> componentsToDisable = new List<ComponentDisableEntry>();
    
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float originalY;
    private float dragPlaneDistance; // 拖动平面距离相机的距离
    public Outline outline;
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        var _ = NpcId; // 触发getter，如果没有ID会自动生成
        // 添加状态机组件
        if (stateMachine == null)
        {
            if (!TryGetComponent<NPCStateMachine>(out stateMachine))
                stateMachine = gameObject.AddComponent<NPCStateMachine>();
        }
    }
    private void Start()
    {
        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        movement = GetComponent<NPCMovement>();
        if (movement == null)
        {
            movement = gameObject.AddComponent<NPCMovement>();
        }


        SubscribeToEvents();

        // 初始化拖动相关字段
        mainCamera = Camera.main;
        originalY = transform.position.y;
        outline.enabled = false;
        
        if (useFixedDragPlane)
        {
            // 计算拖动平面到相机的距离
            dragPlaneDistance = Mathf.Abs(dragPlaneHeight - mainCamera.transform.position.y);
        }
        else
        {
            // 使用物体当前的Y轴位置作为拖动平面
            dragPlaneHeight = originalY;
            dragPlaneDistance = Vector3.Dot(transform.position - mainCamera.transform.position, mainCamera.transform.forward);
        }
    }

    private void Update()
    {
        UpdateMovement();
    }

    private void OnDrawGizmos()
    {
        if (!alwaysShow) return;
#if UNITY_EDITOR
        DrawDebugTextWithHandles();
#endif
    }
    private void OnDestroy()
    {

        UnsubscribeFromEvents();

        // 确保从NPCManager中移除
        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.UnregisterNPC(this);
        }
    }
    #endregion

    #region 初始化和设置

    /// <summary>
    /// 设置NPC数据
    /// </summary>
    /// <param name="npcData"></param>
    /// <returns></returns>
    public NPC SetData(NPCData npcData)
    {
        this.data = npcData;
        this.npcId = npcData.npcId;
        ResetDynamicData();
        return this;
    }

    /// <summary>
    /// 重置NPC的动态数据, 可在生成一个全新的NPC时调用
    /// </summary>
    public void ResetDynamicData()
    {
        // // TODO: 从存档加载NPC数据
        // ChangeState(NPCState.Idle);
        // currentTarget = null; // 解除目标位置
        // AssignedBuilding = null; // 解除建筑分配
        // 清空背包
        if (inventory == null)
        {
            inventory = new Inventory(
                new List<ResourceStack>(),
                Inventory.InventoryAcceptMode.OnlyDefined,
                Inventory.InventoryListFilterMode.None,
                null, null,
                Inventory.InventoryOwnerType.Building,
                data.itemCapacity
            );
        }
        else
        {
            inventory.ownerType = Inventory.InventoryOwnerType.NPC;
        }
        // relationships.Clear(); // 清空社交关系

        // // 重置NavMeshAgent
        // if(navAgent != null){
        //     navAgent.enabled = false;
        //     navAgent.enabled = true;
        // }
    }

    /// <summary>
    /// 从另一个NPC复制数据
    /// </summary>
    /// <param name="other"></param>
    public void CopyFrom(NPC other)
    {
        data = other.data;
        AssignTask(other.AssignedTask);
        stateMachine = other.stateMachine;
        inventory = other.inventory;
        // currentTarget现在由movement组件管理，不需要在这里复制
    }

    public static NPC CreateNPCFromData(NPCInstanceSaveData npcInstanceData)
    {
        return new NPC{
            data = npcInstanceData.npcData,
            npcId = npcInstanceData.npcId
        };
    }
    #endregion

    #region 事件处理
    private void SubscribeToEvents()
    {
        GameEvents.OnNPCSocialInteractionStarted += OnNPCSocialInteractionStarted;
        GameEvents.OnNPCSocialInteractionEnded += OnNPCSocialInteractionEnded;
        GameEvents.OnNPCShouldStartSocialInteraction += OnNPCShouldStartSocialInteraction;
    }
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnNPCSocialInteractionStarted -= OnNPCSocialInteractionStarted;
        GameEvents.OnNPCSocialInteractionEnded -= OnNPCSocialInteractionEnded;
        GameEvents.OnNPCShouldStartSocialInteraction -= OnNPCShouldStartSocialInteraction;
    }

    /// <summary>
    /// NPC应该开始社交互动
    /// </summary>
    /// <param name="args"></param>
    private void OnNPCShouldStartSocialInteraction(NPCEventArgs args)
    {
        // 检查这个事件是否与当前NPC相关
        if (args.npc != this && args.otherNPC != this)
        {
            return; // 不是针对当前NPC的事件，忽略
        }

        // 确认当前NPC处于空闲状态
        if (currentState != NPCState.Idle)
        {
            if (showDebugInfo)
                Debug.LogWarning($"[NPC] {data.npcName} 收到社交事件但不在空闲状态，当前状态：{currentState}");
            return;
        }

        if (showDebugInfo)
            Debug.Log($"[NPC] {data.npcName} 收到社交事件，准备开始社交");

        // 进入准备社交状态
        ChangeState(NPCState.PrepareForSocial);

        // 如果是发起者，转向对方；如果是接收者，也转向对方
        NPC otherNPC = (args.npc == this) ? args.otherNPC : args.npc;
        if (otherNPC != null)
        {
            TurnToPosition(otherNPC.transform.position);
        }
    }

    /// <summary>
    /// NPC开始社交互动
    /// </summary>
    /// <param name="args"></param>
    private void OnNPCSocialInteractionStarted(NPCEventArgs args)
    {
        // TODO: 处理NPC开始社交互动


        // 状态管理


    }

    /// <summary>
    /// NPC结束社交互动
    /// </summary>
    /// <param name="args"></param>
    private void OnNPCSocialInteractionEnded(NPCEventArgs args)
    {
        // TODO: 处理NPC结束社交互动
    }
    #endregion

    #region 状态管理
    public void ChangeState(NPCState newState)
    {
        if (currentState != newState)
        {
            stateMachine.ChangeState(newState);

            // 触发状态改变事件
            NPCEventArgs args = new NPCEventArgs(){
                npc = this,
                oldState = stateMachine.PreviousState,
                newState = newState
            };
            GameEvents.TriggerNPCStateChanged(args);
        }
    }

    #endregion

    #region 唯一标识
    /// <summary>
    /// NPC的唯一标识符
    /// </summary>
    public string NpcId
    {
        get
        {
            // 如果ID为空，生成新的GUID
            if (string.IsNullOrEmpty(npcId))
            {
                npcId = System.Guid.NewGuid().ToString();
                data.npcId = npcId;
                if (showDebugInfo)
                    Debug.Log($"[NPC] 为NPC {data?.npcName} 生成新ID: {npcId}");
            }
            return npcId;
        }
        private set
        {
            npcId = value;
        }
    }
    /// <summary>
    /// 手动设置NPC ID（仅用于加载存档）
    /// </summary>
    public void SetNpcId(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            npcId = id;
            data.npcId = npcId;
            if (showDebugInfo)
                Debug.Log($"[NPC] 设置NPC {data?.npcName} 的ID为: {npcId}");
        }
    }
    /// <summary>
    /// 强制重新生成ID（慎用）
    /// </summary>
    public void RegenerateId()
    {
        string oldId = npcId;
        npcId = System.Guid.NewGuid().ToString();
        data.npcId = npcId;
        if (showDebugInfo)
            Debug.Log($"[NPC] NPC {data?.npcName} ID从 {oldId} 重新生成为 {npcId}");
    }
    #endregion

    #region 工作相关

    [ContextMenu("尝试锁定当前工作")]
    public void LockWork(Building building){
        if(isLocked){
            return;
        }
        // 设置自身锁定状态和锁定任务
        isLocked = true;
        lockedTask = assignedTask;
        // 触发锁定事件
        GameEvents.TriggerNPCLocked(new NPCEventArgs(){npc = this, relatedBuilding = building});
        Debug.Log($"[NPC] {data.npcName} 成功锁定当前工作, 建筑 {building.data.buildingName}");
    }
    public void UnlockWork(Building building){
        if(!isLocked){
            return;
        }
        // 解除锁定状态并清空锁定任务
        isLocked = false;
        lockedTask = TaskInfo.GetNone();

        // 防止NPC不占槽位工作，没有这段的话NPC会继续前往工作但不占用建筑槽位
        // 如果NPC正在MovingToWork状态，也就是正在赶来，则将其改为Idle状态，并清空pendingTask和assignedTask
        if(currentState == NPCState.MovingToWork){
            ChangeState(NPCState.Idle);
            ClearPendingWork();
            AssignTask(TaskInfo.GetNone());
        }

        // 触发解锁事件
        GameEvents.TriggerNPCUnlocked(new NPCEventArgs(){npc = this, relatedBuilding = building});
        Debug.Log($"[NPC] {data.npcName} 成功解锁当前工作, 建筑 {building.data.buildingName}");
    }
    
    public bool CanWorkNow()
    {
        if (TimeManager.Instance == null) return false;
        return TimeManager.Instance.CurrentTime.hour >= data.workTimeStart && TimeManager.Instance.CurrentTime.hour <= data.workTimeEnd;
    }

    public bool ShouldRest()
    {
        if (TimeManager.Instance == null) return false;
        // TODO: 后续考虑其他情况 才 Shouldrest
        return IsRestTime();
    }
    /// <summary>
    /// 获取NPC的工作效率，范围为0-1
    /// </summary>
    /// <returns></returns>
    public float GetWorkEfficiency()
    {
        // 不在工作时间工作效率为0
        if (!CanWorkNow())
            return 0f;

        // 基础工作能力
        float efficiency = data.baseWorkAbility / 100.0f;

        // 1. 特殊词条加成
        efficiency *= GetTraitEfficiencyMultiplier();

        // TODO: 2. 社交关系加成（与同事的好感度）
        // efficiency *= GetRelationshipBonus();

        return efficiency;
    }

    // 词条加成
    private float GetTraitEfficiencyMultiplier()
    {
        float multiplier = 1.0f;

        // 这里写NPC的词条对于工作效率的加成逻辑
        foreach (var trait in data.traits)
        {
            switch (trait)
            {
                case NPCTraitType.FarmExpert:
                    if (assignedTask.building?.data.subType == BuildingSubType.Farm)
                        multiplier *= 1.5f; // 50%加成
                    break;
                case NPCTraitType.LivestockExpert:
                    if (assignedTask.building?.data.subType == BuildingSubType.Ranch)
                        multiplier *= 1.5f;
                    break;
                case NPCTraitType.CheapLabor:
                    multiplier *= 0.8f; // 工作效率降低20%
                    break;
            }
        }

        return multiplier;
    }

    private Coroutine _deliveryRoutine;
    // public IEnumerator StartDelivering(Building building)
    // {
    //     if(showDebugInfo) 
    //         Debug.Log($"NPC {gameObject.name} 开始投送物资到 {building.name}");
    //     while (inventory.TransferTo(building.inventory, transferSpeed))
    //     {
    //         yield return new WaitForSeconds(1f);
    //     }
    //     ChangeState(NPCState.Idle);
    //     AssignedTask = (null, TaskType.None);
    // }

    public void StopDelivering()
    {
        StopCoroutine(_deliveryRoutine);
        ChangeState(NPCState.Idle);
        AssignTask(TaskInfo.GetNone());
    }

    public bool GoWorkAt(Building building){
        if(building == null){
            Debug.LogWarning($"[NPC] 建筑为空");
            return false;
        }
        if(currentState != NPCState.Idle){
            Debug.LogWarning($"[NPC] 当前状态不是空闲状态");
            return false;
        }
        // 如果当前没有任务，则设置任务并进入移动状态
        if(assignedTask.IsNone()){
            pendingTask = new TaskInfo(building, TaskType.Production);
            ChangeState(NPCState.MovingToWork);
            return true;
        }
        return false;
    }
    #endregion

    #region 社交相关
    public void IncreaseRelationship(NPC other, int amount)
    {
        if (NPCManager.Instance?.socialSystem == null) return;
        NPCManager.Instance.socialSystem.IncreaseRelationship(this, other, amount);
    }

    public void DecreaseRelationship(NPC other, int amount)
    {
        if (NPCManager.Instance?.socialSystem == null) return;
        NPCManager.Instance.socialSystem.DecreaseRelationship(this, other, amount);
    }

    public int GetRelationshipWith(NPC other)
    {
        if (NPCManager.Instance?.socialSystem != null)
        {
            return NPCManager.Instance.socialSystem.GetRelationship(this, other);
        }
        return SocialRelationshipManager.DEFAULT_RELATIONSHIP;
    }

    /// <summary>
    /// 获取与所有其他NPC的关系（用于调试和UI显示）
    /// </summary>
    public Dictionary<NPC, int> GetAllRelationships()
    {
        if (NPCManager.Instance?.socialSystem != null)
        {
            return NPCManager.Instance.socialSystem.GetAllRelationshipsFor(this);
        }
        return new Dictionary<NPC, int>();
    }
    #endregion

    #region 特殊能力和词条
    public bool HasTrait(NPCTraitType trait)
    {
        return data.traits != null && data.traits.Contains(trait);
    }

    public float GetTraitBonus(NPCTraitType trait) { return 0f; }
    #endregion

    #region 移动和任务
    private void UpdateMovement()
    {
        // 移动逻辑已转移到NPCMovement中
    }

    /// <summary>
    /// 转向指定方向
    /// </summary>
    /// <param name="direction">目标方向</param>
    public void TurnToDirection(Vector3 direction)
    {
        if (movement != null)
        {
            movement.TurnToDirection(direction);
        }
    }

    /// <summary>
    /// 转向指定位置
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void TurnToPosition(Vector3 targetPosition)
    {
        if (movement != null)
        {
            movement.TurnToPosition(targetPosition);
        }
    }

    /// <summary>
    /// 立即转向目标位置（不使用平滑转向）
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void TurnToPositionImmediate(Vector3 targetPosition)
    {
        if (movement != null)
        {
            movement.TurnToPositionImmediate(targetPosition);
        }
    }

    public void MoveToTarget(Vector3 target)
    {
        if (movement != null)
        {
            movement.MoveToTarget(target);
        }
    }

    public IEnumerator MoveToSocialPosition(Vector3 position, float socialMoveSpeed = 0.5f)
    {
        if (movement != null)
        {
            yield return movement.MoveToSocialPosition(position, socialMoveSpeed);
        }
    }

    public void StartRandomMovement()
    {
        if (movement != null)
        {
            movement.StartRandomMovement();
        }
    }
    public void StopRandomMovement()
    {
        if (movement != null)
        {
            movement.StopRandomMovement();
        }
    }

    public void SetLanded(bool landed)
    {
        if (movement != null)
        {
            movement.isLanded = landed;
        }
    }
    public void AssignTask(TaskInfo task) {
        assignedTask = task;
        if(task.IsNone()){
            return;
        }
        // 如果任务不为空，则触发NPC任务分配事件
        NPCEventArgs args = new NPCEventArgs(){
            npc = this,
            relatedBuilding = task.building
        };
        GameEvents.TriggerNPCAssignedTask(args);
    }

    public bool CanCarryResource(ResourceType type, int amount) { return false; }

    public void CollectResource() { }

    public void DeliverResource() { }
    /// <summary>
    /// 设置当前目标Transform
    /// </summary>
    /// <param name="target">目标Transform</param>
    public void SetCurrentTarget(Transform target)
    {
        if (movement != null)
        {
            movement.SetCurrentTarget(target);
        }
    }

    /// <summary>
    /// 设置当前目标位置
    /// </summary>
    /// <param name="position">目标位置</param>
    public void SetCurrentTargetPosition(Vector3 position)
    {
        if (movement != null)
        {
            movement.SetCurrentTargetPosition(position);
        }
    }
    #endregion

    #region 选择相关
    private bool _canBeSelected = true;
    public bool CanBeSelected {
        get{
            return _canBeSelected;
        } 
        set{
            _canBeSelected = value;
        }
    }
    private Outline _outline;
    public Outline Outline {
        get{
            if(_outline == null){
                _outline = GetComponent<Outline>();
            }
            return _outline;
        }
        set{
            _outline = value;
        }
    }
    public void OnSelect()
    {
        Debug.Log($"[NPC] {data.npcName} 被选中");
        // 不再在这里触发UI打开事件
        // GameEvents.TriggerNPCSelected(this);
        HighlightSelf();
    }
    public void OnDeselect()
    {
        UnhighlightSelf();
    }
    public void HighlightSelf(){
        // 没有的话加一个
        if(Outline == null){
            Outline = gameObject.AddComponent<Outline>();
            Outline.OutlineColor = Color.white;
            Outline.OutlineWidth = 2f;
            Outline.OutlineMode = Outline.Mode.OutlineVisible;

        }

        Outline.enabled = true;
    }
    public void UnhighlightSelf(){
        if(Outline != null){
            Outline.enabled = false;
        }
    }
    #endregion


    #region 存档系统
    // TODO: 这块好像用不到，先留着
    public GameSaveData GetSaveData() { return null; }

    public void LoadFromData(GameSaveData data)
    { 
        var npcInstanceData = data as NPCInstanceSaveData;
        if(npcInstanceData == null){
            Debug.LogError("[NPC] 收到的数据不是NPCInstanceSaveData");
            return;
        }
        SetData(npcInstanceData.npcData);
        SetNpcId(npcInstanceData.npcId);
        if (npcInstanceData.inventorySaveData != null && inventory != null)
        {
            inventory.LoadFromData(npcInstanceData.inventorySaveData);
        }
    }
    #endregion

    #region 调试
    [ContextMenu("Print NPC Relationships")]
    public void PrintNPCRelationships()
    {
        Debug.Log($"[NPC] {data.npcName} 当前对其他NPC的好感度: ==========================");
        var relationships = GetAllRelationships();
        foreach (var relationship in relationships)
        {
            Debug.Log($"{data.npcName} 对 {relationship.Key.data.npcName} 的好感度为: {relationship.Value}");
        }
        Debug.Log($"[NPC] {data.npcName} ====================================================");
    }
    #endregion

    #region 编辑器调试

#if UNITY_EDITOR
    // 在对象上方显示NPC状态、任务和背包信息
    private void DrawDebugTextWithHandles()
    {
        // 构建显示文本
        List<string> displayLines = new List<string>();
        
        // NPC基本信息
        displayLines.Add($"[{data?.npcName}] {currentState}");
        
        // 任务信息
        if (assignedTask != null && assignedTask.building != null)
        {
            displayLines.Add($"[任务] {assignedTask.taskType} -> {assignedTask.building.data?.buildingName}");
        }
        else if (pendingTask != null && pendingTask.building != null)
        {
            displayLines.Add($"[待处理] {pendingTask.taskType} -> {pendingTask.building.data?.buildingName}");
        }
        
        // Inventory资源信息
        if (inventory != null && inventory.currentStacks != null)
        {
            foreach (var resourceStack in inventory.currentStacks)
            {
                if (resourceStack.amount > 0) // 只显示有资源的项目
                {
                    displayLines.Add($"[{resourceStack.displayName}] {resourceStack.amount}/{resourceStack.storageLimit}");
                }
            }
        }
        
        // 计算文本位置（在对象上方）
        Vector3 textPosition = transform.position + Vector3.up * heightOffset;
        
        // 设置文本样式
        GUIStyle style = new GUIStyle();
        style.normal.textColor = textColor;
        style.fontSize = (int)textSize;
        style.alignment = TextAnchor.UpperCenter;
        
        // 绘制多行文本
        string displayText = string.Join("\n", displayLines);
        Handles.Label(textPosition, displayText, style);
    }
#endif

    #endregion

    public TaskInfo GetPendingTask()
    {
        return pendingTask;
    }
    public bool HasPendingTask()
    {
        if (pendingTask == null || pendingTask.building == null || pendingTask.taskType == TaskType.None) return false;
        return true;
    }

    public void SetPendingWork(TaskInfo task)
    {
        this.pendingTask = task;
    }
    public void ClearPendingWork()
    {
        pendingTask = TaskInfo.GetNone();
    }

    public bool IsRestTime()
    {
        if (TimeManager.Instance == null) return false;
        var currentTime = TimeManager.Instance.CurrentTime;
        int restStartHour = data.restTimeStart;
        int restEndHour = data.restTimeEnd;

        // 跨天的情况（例如22:00-6:00）
        if (restStartHour > restEndHour)
        {
            return currentTime.hour >= restStartHour || currentTime.hour < restEndHour;
        }
        // 同一天的情况（例如14:00-18:00）
        else
        {
            return currentTime.hour >= restStartHour && currentTime.hour < restEndHour;
        }
    }

    public void ResetIdleWeight()
    {
        currentIdleWeight = 0f;
    }

    public void IncreaseIdleWeight()
    {
        currentIdleWeight += idleTimeWeight * Time.deltaTime;
    }

    #region 拖动接口实现
    
    public void OnDragStart()
    {
        if (mainCamera == null) return;
        
        isDragging = true;
        outline.enabled = true;

        // 计算鼠标点击位置与物体位置的偏移
        Vector3 mousePosition = GetMouseWorldPositionOnDragPlane();
        offset = transform.position - mousePosition;
        
        // 状态切换时的细节调整：保存当前工作、拆散当前情侣等
        if (currentState == NPCState.Working) SetPendingWork(assignedTask);
        stateMachine.ChangeState(NPCState.Dragging);
        
        // 禁用指定的组件
        foreach (var entry in componentsToDisable)
        {
            if (entry.component != null && entry.disableDuringDrag)
            {
                entry.component.enabled = false;
            }
        }
    }
    
    public void OnDrag()
    {
        if (!isDragging || mainCamera == null) return;
        
        Vector3 mousePosition = GetMouseWorldPositionOnDragPlane();
        Vector3 targetPosition = mousePosition + offset;
        
        // 保持Y轴在拖动平面上
        targetPosition.y = dragPlaneHeight;
        
        transform.position = targetPosition;
    }
    
    public void OnDragEnd()
    {
        if (!isDragging) return;
        
        isDragging = false;
        outline.enabled = false;

        // 重新启用指定的组件
        foreach (var entry in componentsToDisable)
        {
            if (entry.component != null && entry.disableDuringDrag)
            {
                entry.component.enabled = true;
            }
        }
        
        stateMachine.ChangeState(NPCState.Idle);
    }
    
    public bool IsBeingDragged => isDragging;
    
    private Vector3 GetMouseWorldPositionOnDragPlane()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // 创建一个在指定高度的平面
        Plane plane = new Plane(Vector3.up, new Vector3(0, dragPlaneHeight, 0));
        
        // 计算射线与平面的交点
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        // 如果没有交点，返回默认位置
        return mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragPlaneDistance));
    }
    
    #endregion
}