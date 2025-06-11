using UnityEngine;

public class NPCMovingHomeState : NPCStateBase
{
    public NPCMovingHomeState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "正在回家";
        nextState = NPCState.ArrivedHome;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovingHomeState] {npc.data.npcName} 正在回家");
        }
    }
} 