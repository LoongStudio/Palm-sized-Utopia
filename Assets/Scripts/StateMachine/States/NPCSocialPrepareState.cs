using UnityEngine;

public class NPCSocialPrepareState : NPCStateBase
{
    public NPCSocialPrepareState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "准备社交活动";
        nextState = NPCState.MovingToSocial;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSocialPrepareState] {npc.data.npcName} 开始准备社交活动");
        }
    }
} 