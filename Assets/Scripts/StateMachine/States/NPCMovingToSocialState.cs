using UnityEngine;

public class NPCMovingToSocialState : NPCStateBase
{
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
            Debug.Log($"[NPCMovingToSocialState] {npc.data.npcName} 正在前往社交地点");
        }
    }
} 