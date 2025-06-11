using UnityEngine;

public class NPCArrivedHomeState : NPCStateBase
{
    public NPCArrivedHomeState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "已到家";
        nextState = NPCState.Sleeping;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCArrivedHomeState] {npc.data.npcName} 已到家");
        }
    }
} 