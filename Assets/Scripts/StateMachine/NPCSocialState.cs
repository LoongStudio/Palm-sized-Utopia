using UnityEngine;

public class NPCSocialState : NPCStateBase
{
    public NPCSocialState(NPCStateMachine stateMachine, NPC npc) : base(stateMachine, npc)
    {
    }
    protected override void OnEnterState()
    {
        base.OnEnterState();
        animator.SetBool("isSocial", true);
    }
    protected override void OnExitState()
    {
        base.OnExitState();
        animator.SetBool("isSocial", false);
    }
    protected override void OnUpdateState()
    {
        base.OnUpdateState();
    }
    protected override void OnFixedUpdateState()
    {
        base.OnFixedUpdateState();
    }
}