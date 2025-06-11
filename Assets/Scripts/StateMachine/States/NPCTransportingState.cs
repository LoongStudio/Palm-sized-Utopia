using UnityEngine;

public class NPCTransportingState : NPCStateBase
{
    public NPCTransportingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "运输物品";
        nextState = NPCState.WorkComplete;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCTransportingState] {npc.data.npcName} 正在运输物品");
        }
    }
} 