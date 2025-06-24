using UnityEngine;

public class NPCMovingToSocialState : NPCStateBase
{
    // 移动到社交位置超时后，返回空闲状态
    public override float stateExitTime => NPCManager.Instance.socialSystem.socialTimeout;
    public override bool exitStateWhenTimeOut => true;
    public override NPCState nextState => NPCState.Idle;
    
    public NPCMovingToSocialState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "前往社交地点";
        nextState = NPCState.Social;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
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
    }

    protected override void OnUpdateState()
    {
        base.OnUpdateState();
    }

    protected override void OnFixedUpdateState()
    {
        base.OnFixedUpdateState();
    }
} 