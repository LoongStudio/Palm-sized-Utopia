using UnityEngine;

public class NPCWorkCompleteState : NPCStateBase
{
    public NPCWorkCompleteState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "工作完成";
        nextState = NPCState.MovingHome;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCWorkCompleteState] {npc.data.npcName} 工作完成");
        }
    }
} 