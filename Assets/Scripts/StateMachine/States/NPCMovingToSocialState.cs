using UnityEngine;

public class NPCMovingToSocialState : NPCStateBase
{
    // 移动到社交位置超时后，返回空闲状态
    public override float stateExitTime => NPCManager.Instance.socialSystem.socialTimeout;
    public override bool exitStateWhenTimeOut => true;
    public override NPCState nextState => NPCState.Idle;
    private bool isInSocialPosition = false;
    private bool isPartnerInSocialPosition = false;
    private bool inSocialPositionTrigger = false;
    
    public NPCMovingToSocialState(NPCState stateType, NPCStateMachine stateMachine, NPC npc)
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "前往社交地点";
        nextState = NPCState.Social;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        Initialize();

        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovingToSocialState] {npc.data.npcName} 正在前往社交地点: {npc.socialPosition}");
        }
        // 停止移动
        npc.StopRandomMovement();
        
        // NPC到社交位置
        npc.MoveToTarget(npc.socialPosition);
    }

    protected override void OnExitState()
    {
        base.OnExitState();
        // 取消事件
        UnregisterEvent();
    }

    protected override void OnUpdateState()
    {
        base.OnUpdateState();
        // 自己到达社交位置，通知伙伴
        if(npc.isInPosition){
            isInSocialPosition = true; // 标记自己已到达社交地点
            
            // 如果还没有通知社交伙伴，则通知社交伙伴
            if (!inSocialPositionTrigger)
            {
                if (showDebugInfo) Debug.Log($"[NPCMovingToSocialState] {npc.data.npcName} 已到达社交地点");
                inSocialPositionTrigger = true;
                GameEvents.TriggerNPCInSocialPosition(new NPCEventArgs { npc = npc });
            }

        }

        // 如果自己和社交伙伴都到达社交位置，则进入社交状态
        if(isInSocialPosition && isPartnerInSocialPosition){
            if(showDebugInfo)
                Debug.Log($"[NPCMovingToSocialState] {npc.data.npcName} 和 {NPCManager.Instance.socialSystem.GetSocialPartner(npc).data.npcName} 都已到达社交地点");
            npc.ChangeState(NPCState.Social);
        }
    }

    protected override void OnFixedUpdateState()
    {
        base.OnFixedUpdateState();
    }

    private void Initialize(){
        // 初始化值
        isInSocialPosition = false;
        isPartnerInSocialPosition = false;
        // 注册事件
        RegisterEvent();
    }

    private void RegisterEvent(){
        GameEvents.OnNPCInSocialPosition += HandlePartnerInSocialPosition;
    }
    private void UnregisterEvent(){
        GameEvents.OnNPCInSocialPosition -= HandlePartnerInSocialPosition;
    }
    /// <summary>
    /// 处理社交伙伴进入社交位置事件,如果社交伙伴是自己的社交伙伴，则标记伙伴为已到达社交位置
    /// </summary>
    /// <param name="args"></param>
    private void HandlePartnerInSocialPosition(NPCEventArgs args){
        NPC partener = NPCManager.Instance.socialSystem.GetSocialPartner(npc);
        // 如果社交伙伴是自己的社交伙伴，则标记伙伴为已到达社交位置
        if(partener == args.npc){
            isPartnerInSocialPosition = true;
        }
    }
} 