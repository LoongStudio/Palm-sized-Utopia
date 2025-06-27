using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using UnityEditor;
using System;
using UnityEngine.Serialization;

public class NPC : MonoBehaviour, ISaveable
{
    #region 字段声明
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
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
    [SerializeField] private float idleTimeWeight = 0.1f;        // 每秒增加的权重
    [SerializeField] private float currentIdleWeight = 0f;       // 当前累积的权重

    // public Building PendingWork => pendingWork;
    public Vector3? PendingTaskTarget => pendingTask?.building?.transform.position;
    [SerializeField] public TaskInfo assignedTask;               // 分配的建筑
    public float CurrentIdleWeight => currentIdleWeight;

    #endregion
    
    #region Unity生命周期
    private void Awake() {
        var _ = NpcId; // 触发getter，如果没有ID会自动生成
        // 添加状态机组件
        if(stateMachine == null){
            if (!TryGetComponent<NPCStateMachine>(out stateMachine))
                stateMachine = gameObject.AddComponent<NPCStateMachine>();
        }
    }
    private void Start() {
        if(navAgent == null){
            navAgent = GetComponent<NavMeshAgent>();
        }

        movement = GetComponent<NPCMovement>();
        if(movement == null){
            movement = gameObject.AddComponent<NPCMovement>();
        }
        

        SubscribeToEvents();
    }
    
    private void Update()
    {
        UpdateMovement();
    }
    private void OnDestroy() {

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
    public NPC SetData(NPCData npcData){
        this.data = npcData;
        ResetDynamicData();
        return this;
    }
    
    /// <summary>
    /// 重置NPC的动态数据, 可在生成一个全新的NPC时调用
    /// </summary>
    public void ResetDynamicData() {
        // // TODO: 从存档加载NPC数据
        // ChangeState(NPCState.Idle);
        // currentTarget = null; // 解除目标位置
        // AssignedBuilding = null; // 解除建筑分配
        // 清空背包
        if (inventory == null) {
            inventory = new Inventory(
                new List<ResourceStack>(),
                Inventory.InventoryAcceptMode.OnlyDefined,
                Inventory.InventoryListFilterMode.None,
                null, null,
                Inventory.InventoryOwnerType.Building,
                data.itemCapacity
            );
        } else {
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
    public void CopyFrom(NPC other){
        data = other.data;
        assignedTask = other.assignedTask;
        stateMachine = other.stateMachine;
        inventory = other.inventory;
        // currentTarget现在由movement组件管理，不需要在这里复制
    }
    #endregion

    #region 事件处理
    private void SubscribeToEvents() {
        GameEvents.OnNPCSocialInteractionStarted += OnNPCSocialInteractionStarted;
        GameEvents.OnNPCSocialInteractionEnded += OnNPCSocialInteractionEnded;
        GameEvents.OnNPCShouldStartSocialInteraction += OnNPCShouldStartSocialInteraction;
    }
    private void UnsubscribeFromEvents() {
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
            if(showDebugInfo)
                Debug.LogWarning($"[NPC] {data.npcName} 收到社交事件但不在空闲状态，当前状态：{currentState}");
            return;
        }
        
        if(showDebugInfo)
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
    private void OnNPCSocialInteractionStarted(NPCEventArgs args) {
        // TODO: 处理NPC开始社交互动
        
        
        // 状态管理

        
    }

    /// <summary>
    /// NPC结束社交互动
    /// </summary>
    /// <param name="args"></param>
    private void OnNPCSocialInteractionEnded(NPCEventArgs args) {
        // TODO: 处理NPC结束社交互动
    }
    #endregion

    #region 状态管理
    public void ChangeState(NPCState newState) 
    { 
        if (currentState != newState)
        {
            stateMachine.ChangeState(newState);
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
        if (showDebugInfo)
            Debug.Log($"[NPC] NPC {data?.npcName} ID从 {oldId} 重新生成为 {npcId}");
    }
    #endregion

    #region 工作相关
    public bool CanWorkNow() 
    {
        if(TimeManager.Instance == null) return false;
        return TimeManager.Instance.CurrentTime.hour >= data.workTimeStart && TimeManager.Instance.CurrentTime.hour <= data.workTimeEnd;
    }
    
    public bool ShouldRest()
    {
        if(TimeManager.Instance == null) return false;
        // TODO: 后续考虑其他情况 才 Shouldrest
        return IsRestTime();
    }
    
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
        assignedTask = TaskInfo.GetNone();
    }
    #endregion
    
    #region 社交相关
    public void IncreaseRelationship(NPC other, int amount) {
        if(NPCManager.Instance?.socialSystem == null) return;
        NPCManager.Instance.socialSystem.IncreaseRelationship(this, other, amount);
    }
    
    public void DecreaseRelationship(NPC other, int amount) {
        if(NPCManager.Instance?.socialSystem == null) return;
        NPCManager.Instance.socialSystem.DecreaseRelationship(this, other, amount);
    }
    
    public int GetRelationshipWith(NPC other) {
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
    
    public void MoveToTarget(Vector3 target) {
        if (movement != null)
        {
            movement.MoveToTarget(target);
        }
    }
    
    public IEnumerator MoveToSocialPosition(Vector3 position, float socialMoveSpeed = 0.5f) {
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
    public void AssignTask() {}
    
    public bool CanCarryResource(ResourceType type, int amount) { return false; }
    
    public void CollectResource() {}
    
    public void DeliverResource() {}
    #endregion
    
    #region 存档系统
    public SaveData GetSaveData() { return null; }
    
    public void LoadFromData(SaveData data) { }
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

    public TaskInfo GetPendingWork()
    {
        return pendingTask;
    }
    public bool HasPendingWork()
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
        if(TimeManager.Instance == null) return false;
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
} 