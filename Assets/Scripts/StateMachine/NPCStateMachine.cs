using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NPCStateMachine : MonoBehaviour
{
    // 状态字典
    private Dictionary<NPCState, NPCStateBase> states = new Dictionary<NPCState, NPCStateBase>();
    // 组件引用
    private NPC npc;
    public Animator animator;

    [Header("状态机调试")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private NPCState initialState;
    [SerializeField] private NPCStateBase currentState;
    [SerializeField] private NPCStateBase previousState;
    [SerializeField] private float stateTimer = 0f;

    public NPCState CurrentState => GetStateType(currentState);
    public NPCState PreviousState => GetStateType(previousState);

    #region 生命周期
    
    protected virtual void Awake()
    {
        npc = GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError($"[StateMachine] {name} 缺少NPC组件！");
            enabled = false;
            return;
        }
        animator = npc.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"[StateMachine] {name} 缺少Animator组件！");
            enabled = false;
            return;
        }
        
        InitializeStates();
    }
    
    protected virtual void Start()
    {
        // 设置初始状态
        if (states.Count > 0)
        {
            ChangeState(NPCState.Generated);
        }
        else
        {
            if(showDebugLogs){
                Debug.LogError($"[StateMachine] {name} 没有可用的状态！");
            }
        }
    }
    
    protected virtual void Update()
    {
        if (currentState != null)
        {
            stateTimer += Time.deltaTime;
            if(currentState.exitStateWhenTimeOut && stateTimer >= currentState.stateExitTime){
                ChangeState(currentState.nextState);
            }
            currentState.UpdateState();
        }
    }
    private void FixedUpdate()
    {
        currentState?.FixedUpdateState();
    }
    #endregion
    #region 状态初始化
    
    /// <summary>
    /// 初始化所有状态
    /// </summary>
    private void InitializeStates()
    {
        // TODO: 获取或创建各种状态
        states[NPCState.Generated] = new NPCGeneratedState(NPCState.Generated, this, npc);
        states[NPCState.Idle] = new NPCIdleState(NPCState.Idle, this, npc);
        states[NPCState.PrepareForSocial] = new NPCSocialPreparationState(NPCState.PrepareForSocial, this, npc);
        states[NPCState.MovingToSocial] = new NPCMovingToSocialState(NPCState.MovingToSocial, this, npc);
        states[NPCState.Social] = new NPCSocialState(NPCState.Social, this, npc);
        states[NPCState.SocialEndHappy] = new NPCSocialEndHappyState(NPCState.SocialEndHappy, this, npc);
        states[NPCState.SocialEndFight] = new NPCSocialEndFightState(NPCState.SocialEndFight, this, npc);
        // 工作相关状态
        states[NPCState.MovingToWork] = new NPCMovingToWorkState(NPCState.MovingToWork, this, npc);
        states[NPCState.Working] = new NPCWorkingState(NPCState.Working, this, npc);
        states[NPCState.WorkComplete] = new NPCWorkCompleteState(NPCState.WorkComplete, this, npc);
        
        // 回家相关状态
        states[NPCState.MovingHome] = new NPCMovingHomeState(NPCState.MovingHome, this, npc);
        states[NPCState.Sleeping] = new NPCSleepingState(NPCState.Sleeping, this, npc);
        
        if (showDebugLogs)
        {
            Debug.Log($"[StateMachine] {name} 初始化了 {states.Count} 个状态");
        }
    }
    
    #endregion

    #region 状态管理
    /// <summary>
    /// 改变状态
    /// </summary>
    public virtual void ChangeState(NPCState newState)
    {
        if (!states.ContainsKey(newState))
        {
            Debug.LogError($"[StateMachine] {name} 尝试切换到未注册的状态: {newState}");
            return;
        }

        // 检查是否是同一状态
        var newStateComponent = states[newState];
        
        // 如果已经是当前状态，不进行切换
        if (currentState == newStateComponent)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[StateMachine] {name} 已经处于状态 {newState}，跳过切换");
            }
            return;
        }
        

        // 退出当前状态
        if (currentState != null)
        {
            currentState.ExitState();
        }
        // 更新状态信息
        previousState = currentState == null ? new NPCGeneratedState(NPCState.Generated, this, npc) : currentState;
        currentState = newStateComponent;
        stateTimer = 0f;

        // 进入新状态
        currentState.EnterState();

        if(showDebugLogs){
            Debug.Log($"[NPCStateMachine] {name} 状态从 {previousState} 变为 {currentState}");
        }
        TriggerStateChangeEvent();
    }


    #endregion

    #region 状态查询
    /// <summary>
    /// 获取当前状态
    /// </summary>
    public NPCState GetCurrentState() => GetStateType(currentState);
    
    /// <summary>
    /// 获取上一个状态
    /// </summary>
    public NPCState GetPreviousState() => GetStateType(previousState);
    
    /// <summary>
    /// 获取状态持续时间
    /// </summary>
    public float GetStateTimer() => stateTimer;
    #endregion

    #region 事件处理
    /// <summary>
    /// 触发状态变化事件
    /// </summary>
    protected virtual void TriggerStateChangeEvent()
    {
        if (npc != null)
        {
            var eventArgs = new NPCEventArgs
            {
                npc = npc,
                eventType = NPCEventArgs.NPCEventType.StateChanged,
                oldState = GetStateType(previousState),
                newState = GetStateType(currentState),
                timestamp = System.DateTime.Now
            };
            GameEvents.TriggerNPCStateChanged(eventArgs);
        }
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 根据状态组件获取状态类型
    /// </summary>
    private NPCState GetStateType(NPCStateBase state)
    {
        if (state == null) return NPCState.Idle;
        
        foreach (var kvp in states)
        {
            if (kvp.Value == state)
            {
                return kvp.Key;
            }
        }
        
        return NPCState.Idle;
    }
    #endregion
    #region 调试
    [ContextMenu("Print Current State")]
    public void PrintCurrentState()
    {
        Debug.Log($"[NPCStateMachine] {npc.name} 当前状态: {CurrentState}");
    }
    
    #endregion
}

/// <summary>
/// NPC状态接口
/// </summary>
public interface INPCState
{
    IEnumerator EnterState();
    void UpdateState();
    void ExitState();
    bool CanEnter();
    bool CanExit();
}