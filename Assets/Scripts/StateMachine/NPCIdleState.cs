using UnityEngine;

public class NPCIdleState : NPCStateBase
{
    public NPCIdleState(NPCState state, NPCStateMachine stateMachine, NPC npc) : base(state, stateMachine, npc)
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