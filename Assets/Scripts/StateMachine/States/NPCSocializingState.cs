using UnityEngine;

public class NPCSocializingState : NPCStateBase
{
    public NPCSocializingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "进行社交活动";
        nextState = NPCState.SocialSettle;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSocializingState] {npc.data.npcName} 正在进行社交活动");
        }
    }
} 