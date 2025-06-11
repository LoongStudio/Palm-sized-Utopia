using UnityEngine;

public class NPCWorkingState : NPCStateBase
{
    public NPCWorkingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "工作中";
        nextState = NPCState.Transporting;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCWorkingState] {npc.data.npcName} 正在工作");
        }
    }
} 