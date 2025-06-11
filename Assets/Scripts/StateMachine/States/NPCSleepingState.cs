using UnityEngine;

public class NPCSleepingState : NPCStateBase
{
    public NPCSleepingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "睡眠中";
        nextState = NPCState.Idle;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSleepingState] {npc.data.npcName} 正在睡眠");
        }
    }
} 