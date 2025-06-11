using UnityEngine;

public class NPCIdleState : NPCStateBase
{
    public NPCIdleState(NPCState npcState, NPCStateMachine stateMachine, NPC npc) : base(npcState, stateMachine, npc)
    {
    }

    protected override void OnEnterState(){
        base.OnEnterState();
    }
    protected override void OnExitState(){
        base.OnExitState();
    }
    protected override void OnUpdateState(){
        base.OnUpdateState();
    }
    protected override void OnFixedUpdateState(){
        base.OnFixedUpdateState();
    }
}