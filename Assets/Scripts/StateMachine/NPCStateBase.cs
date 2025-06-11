using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC状态基类 - 所有NPC状态的基础类
/// </summary>
public abstract class NPCStateBase
{
    [Header("状态基础信息")]
    [SerializeField] protected bool showDebugInfo = true;
    [SerializeField] protected string stateDescription = "";

    protected NPCStateMachine stateMachine;
    protected NPC npc;
    protected Animator animator;
    
    public virtual float stateExitTime{get;protected set;} = 0f;
    public virtual bool exitStateWhenTimeOut{get;protected set;} = false;
    public virtual NPCState nextState{get;protected set;} = NPCState.Idle;

    // 构造函数
    public NPCStateBase(NPCStateMachine stateMachine, NPC npc){
        this.stateMachine = stateMachine;
        this.npc = npc;
        this.animator = stateMachine.animator;
    }
    
    
    
    #region 状态生命周期
    
    /// <summary>
    /// 进入状态
    /// </summary>
    public virtual void EnterState()
    {
        OnEnterState();
        
    }
    
    /// <summary>
    /// 退出状态
    /// </summary>
    public virtual void ExitState()
    {
        OnExitState();
    }
    
    /// <summary>
    /// 更新状态 - 每帧调用
    /// </summary>
    public virtual void UpdateState()
    {
        OnUpdateState();
    }
    
    /// <summary>
    /// 固定更新 - 物理帧调用
    /// </summary>
    public virtual void FixedUpdateState()
    {
        OnFixedUpdateState();
    }
    
    #endregion
    
    #region 子类重写方法
    
    /// <summary>
    /// 子类进入状态逻辑
    /// </summary>
    protected virtual void OnEnterState() { }
    
    /// <summary>
    /// 子类退出状态逻辑
    /// </summary>
    protected virtual void OnExitState() { }
    
    /// <summary>
    /// 子类更新逻辑
    /// </summary>
    protected virtual void OnUpdateState() { }
    
    /// <summary>
    /// 子类固定更新逻辑
    /// </summary>
    protected virtual void OnFixedUpdateState() { }
    
    /// <summary>
    /// 检查状态转换条件 - 由子类重写
    /// </summary>
    protected virtual void CheckStateTransitions() { }
    
    #endregion
    
    
    #region 辅助方法
    
    /// <summary>
    /// 获取状态描述
    /// </summary>
    public virtual string GetStateDescription()
    {
        if (!string.IsNullOrEmpty(stateDescription))
        {
            return stateDescription;
        }
        
        return $"{GetType().Name} - 描述: {stateDescription}";
    }
    
    
    
    #endregion
    
    #region 暂停和恢复
    
    /// <summary>
    /// 状态机暂停时调用
    /// </summary>
    public virtual void OnPause()
    {
        OnStatePaused();
    }
    
    /// <summary>
    /// 状态机恢复时调用
    /// </summary>
    public virtual void OnResume()
    {
        OnStateResumed();
    }
    
    /// <summary>
    /// 子类暂停逻辑
    /// </summary>
    protected virtual void OnStatePaused() { }
    
    /// <summary>
    /// 子类恢复逻辑
    /// </summary>
    protected virtual void OnStateResumed() { }
    
    #endregion

}