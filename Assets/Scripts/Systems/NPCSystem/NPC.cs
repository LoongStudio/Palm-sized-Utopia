using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;

public class NPC : MonoBehaviour, ISaveable
{
    #region 字段声明
    [Header("基本信息")]
    public NPCData data;
    public Building assignedBuilding;
    
    [Header("社交系统")]
    public Dictionary<NPC, int> relationships = new Dictionary<NPC, int>(); // 好感度系统

    [Header("社交配置")]
    // TODO: 处理这些配置的使用，用SocialSystem代替
    // [SerializeField] private int maxRelationship = 100;
    // [SerializeField] private int minRelationship = 0;
    // [SerializeField] private int defaultRelationship = 50;
    
    [Header("状态管理")]
    public NPCState currentState = NPCState.Idle;
    public NPCState previousState;
    private float stateTimer;

    [Header("任务和背包")]
    public NPCInventory inventory;           // 背包
    public Transform currentTarget;          // 当前目标位置
    public NavMeshAgent navAgent;           // 导航组件

    [Header("移动配置")]
    [SerializeField] private float turnSpeed = 5f;              // 转向速度
    [SerializeField] private float turnThreshold = 10f;         // 转向阈值角度
    [SerializeField] private bool enableTurnBeforeMove = true;  // 是否在移动前转向
    
    private bool isTurning = false;                             // 是否正在转向
    private Vector3 targetDirection;                            // 目标方向
    #endregion
    
    #region Unity生命周期
    private void Start() {
        if(navAgent == null){
            navAgent = GetComponent<NavMeshAgent>();
        }
        // 由NPCManager订阅事件并分发，防止广播风暴
        // SubscribeToEvents();
    }
    
    private void Update()
    {
        UpdateState();
        UpdateMovement();
    }
    private void OnDestroy() {
        // 由NPCManager取消订阅事件，防止广播风暴
        // UnsubscribeFromEvents();
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
        // TODO: 从存档加载NPC数据
        ChangeState(NPCState.Idle);
        currentTarget = null; // 解除目标位置
        assignedBuilding = null; // 解除建筑分配
        // 清空背包
        if(inventory != null){
            inventory.Clear(); 
        }
        relationships.Clear(); // 清空社交关系

        // 重置NavMeshAgent
        if(navAgent != null){
            navAgent.enabled = false;
            navAgent.enabled = true;
        }
    }
    
    /// <summary>
    /// 从另一个NPC复制数据
    /// </summary>
    /// <param name="other"></param>
    public void CopyFrom(NPC other){
        data = other.data;
        assignedBuilding = other.assignedBuilding;
        relationships = other.relationships;
        currentState = other.currentState;
        previousState = other.previousState;
        stateTimer = other.stateTimer;
        inventory = other.inventory;
        currentTarget = other.currentTarget;
    }
    #endregion

    #region 事件处理
    private void SubscribeToEvents() {
        GameEvents.OnNPCSocialInteractionStarted += OnNPCSocialInteractionStarted;
        GameEvents.OnNPCSocialInteractionEnded += OnNPCSocialInteractionEnded;
    }
    private void UnsubscribeFromEvents() {
        GameEvents.OnNPCSocialInteractionStarted -= OnNPCSocialInteractionStarted;
        GameEvents.OnNPCSocialInteractionEnded -= OnNPCSocialInteractionEnded;
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
            previousState = currentState;
            currentState = newState;
            stateTimer = 0f;
            
            // 触发状态变化事件
            var eventArgs = new NPCEventArgs
            {
                npc = this,
                eventType = NPCEventArgs.NPCEventType.StateChanged,
                oldState = previousState,
                newState = currentState,
                timestamp = System.DateTime.Now
            };
            GameEvents.TriggerNPCStateChanged(eventArgs);
        }
    }
    
    private void UpdateState() 
    { 
        stateTimer += Time.deltaTime;

        // TODO: 状态机
        switch (currentState){
            case NPCState.Working:
                break;
            case NPCState.Resting:
                break;
            case NPCState.Idle:
                break;
            case NPCState.Social:
                break;
            default:
                break;
        }
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
        return TimeManager.Instance.CurrentTime.hour >= data.restTimeStart && TimeManager.Instance.CurrentTime.hour <= data.restTimeEnd;
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
                    if (assignedBuilding?.data.subType == BuildingSubType.Farm)
                        multiplier *= 1.5f; // 50%加成
                    break;
                case NPCTraitType.LivestockExpert:
                    if (assignedBuilding?.data.subType == BuildingSubType.Ranch)
                        multiplier *= 1.5f;
                    break;
                case NPCTraitType.CheapLabor:
                    multiplier *= 0.8f; // 工作效率降低20%
                    break;
            }
        }
        
        return multiplier;
    }
    #endregion
    
    #region 社交相关
    public void IncreaseRelationship(NPC other, int amount) {
        if(relationships.ContainsKey(other)){
            relationships[other] += amount;
        }else{
            relationships[other] = amount;
        }
     }
    
    public void DecreaseRelationship(NPC other, int amount) {
        if(relationships.ContainsKey(other)){
            relationships[other] -= amount;
        }else{
            relationships[other] = 0;
        }
    }
    
    public int GetRelationshipWith(NPC other) {
        if(relationships.ContainsKey(other)){
            return relationships[other];
        }else{
            return 0;
        }
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
        // 处理转向逻辑
        if (isTurning)
        {
            UpdateTurning();
        }
    }
    
    /// <summary>
    /// 更新转向逻辑
    /// </summary>
    private void UpdateTurning()
    {
        if (targetDirection == Vector3.zero) return;
        
        // 计算当前方向和目标方向的角度差
        float angle = Vector3.Angle(transform.forward, targetDirection);
        
        // 如果角度差小于阈值，停止转向
        if (angle < turnThreshold)
        {
            isTurning = false;
            OnTurnCompleted();
            return;
        }
        
        // 平滑转向目标方向
        Vector3 newDirection = Vector3.Slerp(transform.forward, targetDirection, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
    
    /// <summary>
    /// 转向完成回调
    /// </summary>
    private void OnTurnCompleted()
    {
        Debug.Log($"[NPC] {data.npcName} 转向完成，开始移动");
        targetDirection = Vector3.zero;
        
        // 转向完成后，开始移动
        if (currentTarget != null && navAgent != null)
        {
            navAgent.SetDestination(currentTarget.position);
        }
    }
    
    /// <summary>
    /// 转向指定方向
    /// </summary>
    /// <param name="direction">目标方向</param>
    public void TurnToDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        
        targetDirection = direction.normalized;
        isTurning = true;
        
        Debug.Log($"[NPC] {data.npcName} 开始转向目标方向");
    }
    
    /// <summary>
    /// 转向指定位置
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void TurnToPosition(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // 忽略Y轴，只在水平面转向
        TurnToDirection(direction);
    }
    
    /// <summary>
    /// 立即转向目标位置（不使用平滑转向）
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void TurnToPositionImmediate(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // 忽略Y轴，只在水平面转向
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            Debug.Log($"[NPC] {data.npcName} 立即转向目标位置");
        }
    }
    
    public void MoveToTarget(Transform target) {
        if(navAgent == null || target == null) return;
        
        currentTarget = target;
        
        if (enableTurnBeforeMove)
        {
            // 先转向目标，转向完成后再开始移动
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0; // 忽略Y轴，只在水平面转向
            
            // 检查是否需要转向
            float angle = Vector3.Angle(transform.forward, direction);
            if (angle > turnThreshold)
            {
                Debug.Log($"[NPC] {data.npcName} 需要转向 {angle:F1}度，开始转向");
                TurnToDirection(direction);
            }
            else
            {
                // 角度差很小，直接移动
                Debug.Log($"[NPC] {data.npcName} 角度差较小({angle:F1}度)，直接移动");
                navAgent.SetDestination(target.position);
            }
        }
        else
        {
            // 直接移动，不转向
            navAgent.SetDestination(target.position);
        }
    }
    
    /// <summary>
    /// 移动到指定位置（重载方法）
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void MoveToPosition(Vector3 targetPosition)
    {
        if(navAgent == null) return;
        
        if (enableTurnBeforeMove)
        {
            // 先转向目标，转向完成后再开始移动
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // 忽略Y轴，只在水平面转向
            
            // 检查是否需要转向
            float angle = Vector3.Angle(transform.forward, direction);
            if (angle > turnThreshold)
            {
                Debug.Log($"[NPC] {data.npcName} 需要转向 {angle:F1}度，开始转向到位置 {targetPosition}");
                // 临时存储目标位置
                currentTarget = new GameObject("TempTarget").transform;
                currentTarget.position = targetPosition;
                TurnToDirection(direction);
            }
            else
            {
                // 角度差很小，直接移动
                Debug.Log($"[NPC] {data.npcName} 角度差较小({angle:F1}度)，直接移动到位置 {targetPosition}");
                navAgent.SetDestination(targetPosition);
            }
        }
        else
        {
            // 直接移动，不转向
            navAgent.SetDestination(targetPosition);
        }
    }
    
    public IEnumerator MoveToSocialPosition(Vector3 position, float socialMoveSpeed = 0.5f) {
        
        // 使用NavMeshAgent移动
        if (navAgent != null)
        {
            Debug.Log($"[NPC] {data.npcName} 开始移动到社交位置: {position}");
            // 保存当前位置和速度
            // Vector3 previousPosition = transform.position;
            float previousSpeed = navAgent.speed;
            // 设置移动速度
            navAgent.speed = socialMoveSpeed;
            
            // 如果启用转向，先转向目标
            if (enableTurnBeforeMove)
            {
                // TODO: 这里需要优化，因为这里会立即转向，不合理
                TurnToPositionImmediate(position); // 社交移动使用立即转向，避免复杂的异步逻辑
            }
            
            // 设置目标位置
            navAgent.SetDestination(position);
            
            // 等待到达目标位置
            while (navAgent.pathPending || navAgent.remainingDistance > 0.5f)
            {
                // 检查是否卡住
                if (navAgent.velocity.magnitude < 0.1f && navAgent.remainingDistance > 0.5f)
                {
                    // 可能卡住了，等待一小段时间
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            // 恢复速度
            navAgent.speed = previousSpeed;
            Debug.Log($"[NPC] {data.npcName} 已到达社交位置");
        } else{
            Debug.LogError($"[NPC] {data.npcName} 没有NavMeshAgent组件，无法移动");
        }
    }
    
    public void AssignTask() {}
    
    public bool CanCarryResource(ResourceType type, int amount) { return false; }
    
    public void CollectResource() {}
    
    public void DeliverResource() {}
    #endregion
    
    #region 存档系统
    public SaveData SaveToData() { return null; }
    
    public void LoadFromData(SaveData data) { }
    #endregion

    #region 调试
    [ContextMenu("Print NPC Relationships")]
    public void PrintNPCRelationships()
    {
        Debug.Log($"[NPC] {data.npcName} 当前对其他NPC的好感度: ==========================");
        foreach (var relationship in relationships)
        {
            Debug.Log($"{data.npcName} 对 {relationship.Key.data.npcName} 的好感度为: {relationship.Value}");
        }
        Debug.Log($"[NPC] {data.npcName} ====================================================");
    }
    #endregion
}