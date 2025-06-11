using UnityEngine;

public class NPCMovingToWorkState : NPCStateBase
{
    public NPCMovingToWorkState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "前往工作地点";
        nextState = NPCState.Working;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovingToWorkState] {npc.data.npcName} 正在前往工作地点");
        }
    }
} 