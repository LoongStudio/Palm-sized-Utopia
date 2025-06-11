using UnityEngine;

public class NPCSocialSettleState : NPCStateBase
{
    public NPCSocialSettleState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "社交活动结束";
        nextState = NPCState.Idle;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSocialSettleState] {npc.data.npcName} 社交活动结束");
        }
    }
} 